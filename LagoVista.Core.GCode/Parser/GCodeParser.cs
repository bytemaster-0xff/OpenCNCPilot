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


        private Dictionary<string, string> _tools = new Dictionary<string, string>();

        //TODO: Removed compiled options
        private Regex GCodeSplitter = new Regex(@"([A-Z])\s*(\-?\d+\.?\d*)");
        private double[] MotionCommands = new double[] { 0, 1, 2, 3, 20, 21, 90, 91 };
        private string ValidWords = "GMXYZSTPIJKFR";
        public List<GCodeCommand> Commands;

        public void Reset()
        {
            State = new ParserState();
            Commands = new List<GCodeCommand>(); //don't reuse, might be used elsewhere
            _tools.Clear();
        }

        public GCodeParser()
        {
            Reset();
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

            //Services.Logger.Log(LogLevel.Message, "GCodeParser_Parse", $"Parsing the GCode File took {sw.ElapsedMilliseconds} ms");
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
                    machineLine.Line = cleanedLine;
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
                Line = line,
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
                Line = line,
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

            foreach(var word in words)
            {
                if(word.Command == 'T')
                {
                    toolName = word.FullWord;
                    if(_tools.ContainsKey(word.FullWord))
                    {
                        toolSize = _tools[toolName];
                    }
                }
            }

            return new ToolChangeCommand()
            {
                Line = line,
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
                motionMode = (int)words.First().Parameter;
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
                State.Feed = feedRateCommand.Parameter;
                words.Remove(feedRateCommand);
            }

            var spindleRPM = words.Where(wrd => wrd.Command == 'S').FirstOrDefault();
            if (spindleRPM != null)
            {
                State.SpindleRPM = spindleRPM.Parameter;
                words.Remove(spindleRPM);
            }
            

            double pauseTime = 0.0;

            var pauseParameter = words.Where(wrd => wrd.Command == 'P').FirstOrDefault();
            if (pauseParameter != null)
            {
                pauseTime = Convert.ToDouble(pauseParameter.Parameter);
                words.Remove(pauseParameter);
            }

            try
            {
                switch (motionMode)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        var motion = (motionMode <= 1) ? ParseLine(words, motionMode, EndPos) : ParseArc(words, motionMode, EndPos, UnitMultiplier);
                        motion.Line = line;
                        motion.Feed = State.Feed;
                        motion.SpindlePRM = State.SpindleRPM;
                        motion.LineNumber = lineNumber;
                        State.Position = EndPos;

                        return motion;

                    case 4:
                        return new OtherCode()
                        {
                            PauseTime = pauseTime,
                            Line = line,
                        };

                    case 17: /* XY Plane Selection */
                    case 80: /* Cancel Canned Cycle */
                    case 98: /* Return to initial Z Position */
                        return new OtherCode()
                        {
                            Line = line,
                        };


                    case 81:
                        var drillCode = new GCodeDrill()
                        {
                            Start = State.Position,
                            End = State.Position,
                            LineNumber = lineNumber,
                            Feed = State.Feed
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
