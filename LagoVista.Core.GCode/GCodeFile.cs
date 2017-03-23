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
        public ReadOnlyCollection<GCodeCommand> Commands { get; private set; }
        public string FileName { get; set; }
        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }
        public Vector3 Size { get; private set; }

        public double TravelDistance { get; private set; } = 0;

        private GCodeFile(List<GCodeCommand> commands)
        {

            Commands = new ReadOnlyCollection<GCodeCommand>(commands);

            Vector3 min = Vector3.MaxValue, max = Vector3.MinValue;

            foreach (GCodeMotion m in Enumerable.Concat(Commands.OfType<GCodeLine>(), Commands.OfType<GCodeArc>().SelectMany(a => a.Split(0.1))))
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
            List<GCodeCommand> newFile = new List<GCodeCommand>();

            foreach (GCodeCommand c in Commands)
            {
                if (c is GCodeMotion)
                {
                    newFile.AddRange(((GCodeMotion)c).Split(length));
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
            var newFile = new List<GCodeCommand>();

            foreach (var cmd in Commands)
            {
                if (cmd is GCodeArc)
                {
                    foreach (var segment in ((GCodeArc)cmd).Split(length).Cast<GCodeArc>())
                    {
                        var ilne = new GCodeLine()
                        {
                            Start = segment.Start,
                            End = segment.End,
                            Feed = segment.Feed,
                            Rapid = false
                        };
                        newFile.Add(ilne);
                    }
                }
                else
                {
                    newFile.Add(cmd);
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
