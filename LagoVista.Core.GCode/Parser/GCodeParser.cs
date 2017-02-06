﻿using System;
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
        private string ValidWords = "GMXYZIJKFR";
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
            int i = 1;

            var sw = System.Diagnostics.Stopwatch.StartNew();

            foreach (string linei in file)
            {
                var line = CleanupLine(linei, i);

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var motionLine = Parse(line.ToUpper(), i);
                if (motionLine != null)
                {
                    Commands.Add(motionLine);
                }
            
                i++;
            }

            sw.Stop();

            Services.Logger.Log(LogLevel.Message, "GCodeParser_Parse", $"Parsing the GCode File took {sw.ElapsedMilliseconds} ms");
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

                line = line.Remove(start, end - start);
            }

            return line;
        }

        public GCodeMotion Parse(string line, int lineNumber)
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
            }

            if (motionMode < 0)
                throw new ParseException("No Motion Mode active", lineNumber);

            var UnitMultiplier = (State.Unit == ParseUnit.Metric) ? 1 : 25.4;

            var EndPos = FindEndPosition(words, UnitMultiplier);

            var feedRateCommand = words.Where(wrd => wrd.Command == 'F').FirstOrDefault();
            if (feedRateCommand != null)
            {
                State.Feed = feedRateCommand.Parameter;
                words.Remove(feedRateCommand);
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
