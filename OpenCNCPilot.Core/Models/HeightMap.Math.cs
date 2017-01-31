using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Models
{
    public partial class HeightMap
    {
        public double InterpolateZ(double x, double y)
        {
            if (x > Max.X || x < Min.X || y > Max.Y || y < Min.Y)
                return MaxHeight;

            x -= Min.X;
            y -= Min.Y;

            x /= GridX;
            y /= GridY;

            int iLX = (int)Math.Floor(x);   //lower integer part
            int iLY = (int)Math.Floor(y);

            int iHX = (int)Math.Ceiling(x); //upper integer part
            int iHY = (int)Math.Ceiling(y);

            double fX = x - iLX;             //fractional part
            double fY = y - iLY;

            double linUpper = Points[iHX, iHY].Value * fX + Points[iLX, iHY].Value * (1 - fX);       //linear immediates
            double linLower = Points[iHX, iLY].Value * fX + Points[iLX, iLY].Value * (1 - fX);

            return linUpper * fY + linLower * (1 - fY);     //bilinear result
        }

        public Core.GCode.GCodeFile ApplyHeightMap(Core.GCode.GCodeFile file)
        {
            double segmentLength = Math.Min(GridX, GridY);

            var newToolPath = new List<Core.GCode.Commands.GCodeCommand>();

            foreach (var command in file.Commands)
            {
                if (command is Core.GCode.Commands.MCode)
                {
                    newToolPath.Add(command);
                    continue;
                }
                else
                {
                    Core.GCode.Commands.GCodeMotion m = (Core.GCode.Commands.GCodeMotion)command;

                    foreach (var subMotion in m.Split(segmentLength))
                    {
                        var startZOffset = InterpolateZ(subMotion.Start.X, subMotion.Start.Y);
                        subMotion.Start = new Core.Models.Drawing.Vector3(subMotion.Start.X, subMotion.Start.Y, subMotion.Start.Z + startZOffset);

                        var endZOffset = InterpolateZ(subMotion.End.X, subMotion.End.Y);
                        subMotion.End = new Core.Models.Drawing.Vector3(subMotion.End.X, subMotion.End.Y, subMotion.End.Z + endZOffset);

                        newToolPath.Add(subMotion);
                    }
                }
            }

            return Core.GCode.GCodeFile.FromCommands(newToolPath);
        }

    }
}
