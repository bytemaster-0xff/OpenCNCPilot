using LagoVista.Core.GCode.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Models
{
    public partial class HeightMap
    {
        /// <summary>
        /// Returns a point based on it's X, Y index
        /// </summary>
        /// <param name="xIndex">X Index into list</param>
        /// <param name="yIndex">Y Index into list</param>
        /// <returns></returns>
        private HeightMapProbePoint GetPoint(int xIndex, int yIndex)
        {
            return Points.Where(pnt => pnt.XIndex == xIndex && pnt.YIndex == yIndex).FirstOrDefault();
        }

        public double InterpolateZ(double x, double y)
        {
            if (x > Max.X || x < Min.X || y > Max.Y || y < Min.Y)
                return MaxHeight;

            x -= Min.X;
            y -= Min.Y;

            x /= GridX;
            y /= GridY;

            var iLX = (int)Math.Floor(x);   //lower integer part
            var iLY = (int)Math.Floor(y);

            var iHX = (int)Math.Ceiling(x); //upper integer part
            var iHY = (int)Math.Ceiling(y);

            var fX = x - iLX;             //fractional part
            var fY = y - iLY;

            //TODO: Should be able to use Linq and pull via position
            var linUpper = GetPoint(iHX, iHY).Point.Z * fX + GetPoint(iLX, iHY).Point.Z * (1 - fX);       //linear immediates
            var linLower = GetPoint(iHX, iLY).Point.Z * fX + GetPoint(iLX, iLY).Point.Z * (1 - fX);

            return linUpper * fY + linLower * (1 - fY);     //bilinear result
        }

        public Core.GCode.GCodeFile ApplyHeightMap(Core.GCode.GCodeFile file)
        {
            if (!Completed)
            {

            }

            var segmentLength = Math.Min(GridX, GridY);

            var newToolPath = new List<Core.GCode.Commands.GCodeCommand>();

            foreach (var command in file.Commands)
            {
                if (command is GCodeMotion)
                {
                    var m = command as GCodeMotion;

                    foreach (var subMotion in m.Split(segmentLength))
                    {
                        var startZOffset = InterpolateZ(subMotion.Start.X, subMotion.Start.Y);
                        subMotion.Start = new Core.Models.Drawing.Vector3(subMotion.Start.X, subMotion.Start.Y, subMotion.Start.Z + startZOffset);

                        var endZOffset = InterpolateZ(subMotion.End.X, subMotion.End.Y);
                        subMotion.End = new Core.Models.Drawing.Vector3(subMotion.End.X, subMotion.End.Y, subMotion.End.Z + endZOffset);

                        newToolPath.Add(subMotion);
                    }
                }
                else
                {
                    newToolPath.Add(command);
                }
            }

            return Core.GCode.GCodeFile.FromCommands(newToolPath);
        }
    }
}
