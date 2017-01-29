using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Collections.ObjectModel;
using System;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.GCode.Commands;

namespace LagoVista.Core.GCode
{
    public partial class GCodeFile
    {
        public ReadOnlyCollection<Command> Commands { get; private set; }
        public string FileName = string.Empty;
        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }
        public Vector3 Size { get; private set; }

        public double TravelDistance { get; private set; } = 0;

        private GCodeFile(List<Command> commands)
        {

            Commands = new ReadOnlyCollection<Command>(commands);

            Vector3 min = Vector3.MaxValue, max = Vector3.MinValue;

            foreach (Motion m in Enumerable.Concat(Commands.OfType<GCodeLine>(), Commands.OfType<GCodeArc>().SelectMany(a => a.Split(0.1))))
            {
                for (int i = 0; i < 3; i++)
                {
                    if (m.End[i] > max[i])
                        max[i] = m.End[i];

                    if (m.End[i] < min[i])
                        min[i] = m.End[i];
                }

                TravelDistance += m.Length;
            }

            Max = max;
            Min = min;

            Vector3 size = Max - Min;

            for (int i = 0; i < 3; i++)
            {
                if (size[i] < 0)
                    size[i] = 0;
            }

            Size = size;
        }

        public async void Save(string path)
        {
            await Services.Storage.WriteAllLinesAsync(path, GetGCode());
        }


        public GCodeFile Split(double length)
        {
            List<Command> newFile = new List<Command>();

            foreach (Command c in Commands)
            {
                if (c is Motion)
                {
                    newFile.AddRange(((Motion)c).Split(length));
                }
                else
                {
                    newFile.Add(c);
                }
            }

            return new GCodeFile(newFile);
        }

        public GCodeFile ArcsToLines(double length)
        {
            List<Command> newFile = new List<Command>();

            foreach (Command c in Commands)
            {
                if (c is GCodeArc)
                {
                    foreach (var segment in ((GCodeArc)c).Split(length).Cast<GCodeArc>())
                    {
                        var l = new GCodeLine();
                        l.Start = segment.Start;
                        l.End = segment.End;
                        l.Feed = segment.Feed;
                        l.Rapid = false;
                        newFile.Add(l);
                    }
                }
                else
                {
                    newFile.Add(c);
                }
            }

            return new GCodeFile(newFile);
        }

        public TimeSpan EstimatedRunTime
        {
            get
            {
                var runTimeMS = Commands.Sum(cmd => cmd.EstimatedRunTime.TotalMilliseconds);
                return TimeSpan.FromMilliseconds(runTimeMS);
            }
        }

    }
}
