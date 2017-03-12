using LagoVista.Core.Models.Drawing;
using System;
using System.Diagnostics;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class SMD
    {
        public int Layer { get; set; }
        public string Name { get; set; }
        public double OriginX { get; set; }
        public double OriginY { get; set; }

        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double DX { get; set; }
        public double DY { get; set; }
        public double? Roundness { get; set; }
        public string RotateStr { get; set; }

        public Package Package { get; set; }

        public SMD ApplyRotation(double angle)
        {
            var smd = this.MemberwiseClone() as SMD;
            if (angle == 0)
            {
                return smd;
            }

            //TODO: Why do we ignore the rotation at the package level?  If it's not 90, do we rotate then?
            /*if (RotateStr.StartsWith("R"))
            {
                if (String.IsNullOrEmpty(RotateStr))
                {
                    return pad;
                };

                double angle;
                if (double.TryParse(RotateStr.Substring(1), out angle))
                {*/
            var rotatedStart = new Point2D<double>(X1, Y1).Rotate(angle);
            var rotatedEnd = new Point2D<double>(X2, Y2).Rotate(angle);

            smd.X1 = rotatedStart.X;
            smd.Y1 = rotatedStart.Y;

            smd.X2 = rotatedEnd.X;
            smd.Y2 = rotatedEnd.Y;


            //pad.OriginX = Math.Round(rotated.X, 6);
            //pad.OriginY = Math.Round(rotated.Y, 6);






            /*}
        }*/

            return smd;
        }

        public static SMD Create(XElement element)
        {
            var smd = new SMD()
            {
                Layer = element.GetInt32("layer"),
                Name = element.GetString("name"),
                OriginX = element.GetDouble("x"),
                OriginY = element.GetDouble("y"),
                DX = element.GetDouble("dx"),
                DY = element.GetDouble("dy"),
                Roundness = element.GetDoubleNullable("roundness"),
                RotateStr = element.GetString("rot")
            };



            return smd;
        }
    }
}
