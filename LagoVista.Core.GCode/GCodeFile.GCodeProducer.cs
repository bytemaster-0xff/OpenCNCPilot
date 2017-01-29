using LagoVista.Core.GCode.Commands;
using LagoVista.Core.GCode.Parser;
using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Core.GCode
{
    public partial class GCodeFile
    {
        public List<string> GetGCode()
        {
            List<string> GCode = new List<string>(Commands.Count + 1) { "G90 G91.1 G21 G17" };

            var nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";   //prevent problems with international versions of windows (eg Germany would write 25.4 as 25,4 which is not compatible with standard GCode)

            var State = new ParserState();

            foreach (var c in Commands)
            {
                if (c is Motion)
                {
                    var m = c as Motion;

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

                    var Center = new Vector3(a.U, a.V, 0).RollComponents((int)a.Plane) - State.Position;

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
    }
}
