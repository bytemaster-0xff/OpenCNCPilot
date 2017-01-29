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


        public List<string> GetGCode()
        {
            List<string> GCode = new List<string>(Commands.Count + 1) { "G90 G91.1 G21 G17" };

            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";   //prevent problems with international versions of windows (eg Germany would write 25.4 as 25,4 which is not compatible with standard GCode)

            ParserState State = new ParserState();

            foreach (Command c in Commands)
            {
                if (c is Motion)
                {
                    Motion m = c as Motion;

                    if (m.Feed != State.Feed)
                    {
                        GCode.Add(string.Format(nfi, "F{0:0.###}", m.Feed));

                        State.Feed = m.Feed;
                    }
                }

                if (c is GCodeLine)
                {
                    var l = c as GCodeLine;

                    string code = l.Rapid ? "G0" : "G1";

                    if (State.Position.X != l.End.X)
                        code += string.Format(nfi, "X{0:0.###}", l.End.X);
                    if (State.Position.Y != l.End.Y)
                        code += string.Format(nfi, "Y{0:0.###}", l.End.Y);
                    if (State.Position.Z != l.End.Z)
                        code += string.Format(nfi, "Z{0:0.###}", l.End.Z);

                    GCode.Add(code);

                    State.Position = l.End;

                    continue;
                }

                if (c is GCodeArc)
                {
                    var a = c as GCodeArc;

                    if (State.Plane != a.Plane)
                    {
                        switch (a.Plane)
                        {
                            case ArcPlane.XY:
                                GCode.Add("G17");
                                break;
                            case ArcPlane.YZ:
                                GCode.Add("G19");
                                break;
                            case ArcPlane.ZX:
                                GCode.Add("G18");
                                break;
                        }
                        State.Plane = a.Plane;
                    }

                    string code = a.Direction == ArcDirection.CW ? "G2" : "G3";

                    if (State.Position.X != a.End.X)
                        code += string.Format(nfi, "X{0:0.###}", a.End.X);
                    if (State.Position.Y != a.End.Y)
                        code += string.Format(nfi, "Y{0:0.###}", a.End.Y);
                    if (State.Position.Z != a.End.Z)
                        code += string.Format(nfi, "Z{0:0.###}", a.End.Z);

                    Vector3 Center = new Vector3(a.U, a.V, 0).RollComponents((int)a.Plane) - State.Position;

                    if (Center.X != 0 && a.Plane != ArcPlane.YZ)
                        code += string.Format(nfi, "I{0:0.###}", Center.X);
                    if (Center.Y != 0 && a.Plane != ArcPlane.ZX)
                        code += string.Format(nfi, "J{0:0.###}", Center.Y);
                    if (Center.Z != 0 && a.Plane != ArcPlane.XY)
                        code += string.Format(nfi, "K{0:0.###}", Center.Z);

                    GCode.Add(code);
                    State.Position = a.End;

                    continue;
                }

                if (c is MCode)
                {
                    GCode.Add($"M{((MCode)c).Code}");

                    continue;
                }
            }

            return GCode;
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

    }
}
