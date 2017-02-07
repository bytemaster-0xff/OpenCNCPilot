using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.GCode.Commands;

namespace LagoVista.Core.GCode.Parser
{
    public partial class GCodeParser
    {
        public ParserState State { get; private set; }

        //TODO: Removed compiled options
        private Regex GCodeSplitter = new Regex(@"([A-Z])\s*(\-?\d+\.?\d*)");
        private double[] MotionCommands = new double[] { 0, 1, 2, 3 };
        private string ValidWords = "GMXYZSTPIJKFR";
        public List<GCodeCommand> Commands;

        public void Reset()
        {
            State = new ParserState();
            Commands = new List<GCodeCommand>(); //don't reuse, might be used elsewhere
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
            }

            sw.Stop();

            Services.Logger.Log(LogLevel.Message, "GCodeParser_Parse", $"Parsing the GCode File took {sw.ElapsedMilliseconds} ms");
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
                else if (cleanedLine.StartsWith("M"))
                {
                    var machineLine = ParseMachineCommand(cleanedLine.ToUpper(), lineIndex);
                    if(machineLine != null)
                    {
                        machineLine.SetComment(GetComment(line));
                        return machineLine;
                    }
                }
                else
                {

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

        public GCodeMotion ParseMotionLine(string line, int lineNumber)
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

            var dwellCommand = words.Where(wrd => wrd.Command == 'P').FirstOrDefault();
            if (dwellCommand != null)
            {
                //TODO: Do we care?
               // State.Feed = feedRateCommand.Parameter;
                words.Remove(dwellCommand);
            }

            try
            {
                var motion = (motionMode <= 1) ? ParseLine(words, motionMode, EndPos) : ParseArc(words, motionMode, EndPos, UnitMultiplier);
                motion.Line = line;
                motion.Feed = State.Feed;
                motion.LineNumber = lineNumber;
                State.Position = EndPos;

                return motion;
            }
            catch (Exception ex)
            {
                throw new ParseException(ex.Message, lineNumber);
            }
        }
    }
}
