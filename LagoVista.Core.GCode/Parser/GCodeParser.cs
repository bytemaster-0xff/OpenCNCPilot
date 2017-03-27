using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.GCode.Commands;
using System.Diagnostics;

namespace LagoVista.Core.GCode.Parser
{
    public partial class GCodeParser
    {
        public bool Diagnostics { get; set; } = true;

        public ParserState State { get; private set; }

        ILogger _logger;

        private Dictionary<string, string> _tools = new Dictionary<string, string>();

        //TODO: Removed compiled options
        private Regex GCodeSplitter = new Regex(@"([A-Z])\s*(\-?\d+\.?\d*)");
        private double[] MotionCommands = new double[] { 0, 1, 2, 3, 38.2, 38.3, 38.4, 38.5, 20, 21, 90, 91 };
        private string ValidWords = "GMXYZSTPIJKFR";
        public List<GCodeCommand> Commands;

        public void Reset()
        {
            State = new ParserState();
            Commands = new List<GCodeCommand>(); //don't reuse, might be used elsewhere
            _tools.Clear();
        }

        public GCodeParser(ILogger logger)
        {
            Reset();
            _logger = logger;
        }

        public async void ParseFile(string path)
        {
            var lines = await Services.Storage.ReadAllLinesAsync(path);
            Parse(lines);
        }

        public void Parse(IEnumerable<string> file)
        {
            int lineIndex = 1;

            var sw = System.Diagnostics.Stopwatch.StartNew();

            foreach (string line in file)
            {
                var command = ParseLine(line, lineIndex);
                if (command != null)
                {
                    Commands.Add(command);

                    lineIndex++;
                }
                else
                {
                    Debug.WriteLine("Skipping Line: " + line);
                }
            }

            sw.Stop();
        }

        public GCodeCommand ParseLine(string line, int lineIndex)
        {
            var cleanedLine = CleanupLine(line, lineIndex);

            if (!string.IsNullOrWhiteSpace(cleanedLine))
            {
                if (cleanedLine.StartsWith("G"))
                {
                    var motionLine = ParseMotionLine(cleanedLine.ToUpper(), lineIndex);
                    if (motionLine != null)
                    {
                        motionLine.SetComment(GetComment(line));
                        return motionLine;
                    }
                }
                else if (cleanedLine.StartsWith("T") || cleanedLine.StartsWith("M06") || cleanedLine.StartsWith("M6"))
                {
                    var machineLine = ParseToolChangeCommand(cleanedLine.ToUpper(), lineIndex);
                    if (machineLine != null)
                    {
                        machineLine.SetComment(GetComment(line));
                        return machineLine;
                    }
                }
                else if (cleanedLine.StartsWith("M"))
                {
                    var machineLine = ParseMachineCommand(cleanedLine.ToUpper(), lineIndex);
                    if (machineLine != null)
                    {
                        machineLine.SetComment(GetComment(line));
                        return machineLine;
                    }
                }
                else if (cleanedLine.StartsWith("S"))
                {
                    var machineLine = new OtherCode();
                    machineLine.LineNumber = lineIndex;
                    machineLine.OriginalLine = cleanedLine;
                    return machineLine;
                }
            }
            return null;
        }

        public string GetComment(string line)
        {
            int commentIndex = line.IndexOf(';');

            return commentIndex > -1 ? line.Substring(commentIndex + 1) : "";
        }

        public string CleanupLine(string line, int lineNumber)
        {
            int commentIndex = line.IndexOf(';');

            if (commentIndex > -1)
                line = line.Remove(commentIndex);

            int start = -1;

            var toolRegEx1 = new Regex(@"\( (?'ToolNumber'-?T[0-9]*) : (?'ToolSize'-?[0-9\.]*) \)");
            var toolMatch1 = toolRegEx1.Match(line);
            if (toolMatch1.Success)
            {
                var toolNumber = toolMatch1.Groups["ToolNumber"].Value;
                var toolSize = toolMatch1.Groups["ToolSize"].Value;
                if (_tools.ContainsKey(toolNumber))
                {
                    _tools.Remove(toolNumber);
                }

                _tools.Add(toolNumber, toolSize);

                return String.Empty;
            }

            var toolRegEx2 = new Regex(@"\( (?'ToolNumber'-?T[0-9]*) *(?'ToolSizeMM'-?[0-9\.]*)mm *(?'ToolSizeIN'-?[0-9\.]*)in.*\)");
            var toolMatch2 = toolRegEx2.Match(line);
            if (toolMatch2.Success)
            {
                var toolNumber = toolMatch2.Groups["ToolNumber"].Value;
                var toolSize = toolMatch2.Groups["ToolSizeMM"].Value;
                if (_tools.ContainsKey(toolNumber))
                {
                    _tools.Remove(toolNumber);
                }

                _tools.Add(toolNumber, toolSize);

                return String.Empty;
            }


            while ((start = line.IndexOf('(')) != -1)
            {
                int end = line.IndexOf(')');

                if (end < start)
                    throw new ParseException("mismatched parentheses", lineNumber);

                line = line.Remove(start, (end - start) + 1);
            }

            return line;
        }

        public OtherCode ParseOtherCode(string line, int lineNumber)
        {
            return new OtherCode()
            {
                OriginalLine = line,
                LineNumber = lineNumber
            };
        }

        public MCode ParseMachineCommand(string line, int lineNumber)
        {
            var words = FindWords(line);
            Validate(words);
            Prune(words, line, lineNumber);
            if (words.Count == 0)
            {
                return null;
            }

            return new MCode()
            {
                OriginalLine = line,
                LineNumber = lineNumber
            };
        }

        public ToolChangeCommand ParseToolChangeCommand(string line, int lineNumber)
        {
            var words = FindWords(line);
            Validate(words);
            Prune(words, line, lineNumber);
            if (words.Count == 0)
            {
                return null;
            }

            var toolName = "??";
            var toolSize = "??";

            foreach (var word in words)
            {
                if (word.Command == 'T')
                {
                    toolName = word.FullWord;
                    if (_tools.ContainsKey(word.FullWord))
                    {
                        toolSize = _tools[toolName];
                    }
                }
            }

            return new ToolChangeCommand()
            {
                OriginalLine = line,
                LineNumber = lineNumber,
                ToolName = toolName,
                ToolSize = toolSize
            };
        }

        public GCodeCommand ParseMotionLine(string line, int lineNumber)
        {
            var words = FindWords(line);

            Validate(words);

            Prune(words, line, lineNumber);

            if (words.Count == 0)
            {
                return null;
            }

            var motionMode = State.LastMotionMode;

            if (words.First().Command == 'G')
            {
                motionMode = words.First().Parameter;
                State.LastMotionMode = motionMode;
                words.RemoveAt(0);

                if (motionMode < 0)
                    throw new ParseException("No Motion Mode active", lineNumber);
            }

            var UnitMultiplier = (State.Unit == ParseUnit.Metric) ? 1 : 25.4;

            var EndPos = FindEndPosition(words, UnitMultiplier);

            var feedRateCommand = words.Where(wrd => wrd.Command == 'F').FirstOrDefault();
            if (feedRateCommand != null)
            {
                words.Remove(feedRateCommand);
            }

            var spindleRPM = words.Where(wrd => wrd.Command == 'S').FirstOrDefault();
            if (spindleRPM != null)
            {
                words.Remove(spindleRPM);
            }

            TimeSpan pauseTime = TimeSpan.Zero;

            var pauseParameter = words.Where(wrd => wrd.Command == 'P').FirstOrDefault();
            if (pauseParameter != null)
            {
                pauseTime =TimeSpan.FromSeconds(Convert.ToDouble(pauseParameter.Parameter));
                words.Remove(pauseParameter);
            }

            try
            {
                switch ((int)motionMode)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        var motion = (motionMode <= 1) ? ParseLine(words, motionMode, EndPos) : ParseArc(words, motionMode, EndPos, UnitMultiplier);
                        motion.Command = $"G{motionMode}";
                        motion.OriginalLine = line;
                        motion.PreviousFeed = State.Feed;
                        motion.PreviousSpindleRPM = State.SpindleRPM;
                        if (feedRateCommand != null)
                        {
                            State.Feed = feedRateCommand.Parameter;
                            motion.Feed = State.Feed;
                        }
                        else
                        {
                            motion.Feed = motion.PreviousFeed;
                        }

                        if (spindleRPM != null)
                        {
                            State.SpindleRPM = spindleRPM.Parameter;
                            motion.SpindleRPM = State.SpindleRPM;
                        }
                        else
                        {
                            motion.SpindleRPM = motion.PreviousSpindleRPM;
                        }

                        motion.LineNumber = lineNumber;
                        State.Position = EndPos;

                        return motion;

                    case 4:
                        return new GCodeDwell()
                        {
                            DwellTime = pauseTime,
                            OriginalLine = line,
                            Command = $"G4"
                        };
                    case 38:
                        var probeCommand = new GCodeProbe()
                        {
                            OriginalLine = line,
                            Command = $"G{motionMode}"
                        };
                        if (feedRateCommand != null)
                        {
                            probeCommand.Feed = feedRateCommand.Parameter;
                        }

                        return probeCommand;
                    case 17: /* XY Plane Selection */
                    case 80: /* Cancel Canned Cycle */
                    case 98: /* Return to initial Z Position */
                        return new OtherCode()
                        {
                            OriginalLine = line,
                            Command = $"G{motionMode}"
                        };


                    case 81:
                        var drillCode = new GCodeDrill()
                        {
                            Command = "G81",
                            Start = State.Position,
                            End = State.Position,
                            LineNumber = lineNumber,
                            Feed = State.Feed,
                            OriginalLine = line
                        };
                        break;

                }

                return null;


            }
            catch (Exception ex)
            {
                throw new ParseException(ex.Message, lineNumber);
            }
        }
    }
}
