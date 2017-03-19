using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class Pad
    {
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double OriginX { get; set; }
        public double OriginY { get; set; }
        public double DrillDiameter { get; set; }
        public string Shape { get; set; }
        public string RotateStr { get; set; }

        public static Pad Create(XElement element)
        {
            return new Pad()
            {
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                OriginX = element.GetDouble("x"),
                OriginY = element.GetDouble("y"),
                DrillDiameter = element.GetDouble("drill"),
                Name = element.GetString("name"),
                Shape = element.GetString("shape", "Circle"),
                RotateStr = element.GetString("rot")
            };
        }

        public Pad ApplyRotation(double angle)
        {
            var pad = this.MemberwiseClone() as Pad;
            if(angle == 0)
            {
                return pad;
            }
            var rotated = new Point2D<double>(pad.X, pad.Y).Rotate(angle);

            pad.X = Math.Round(rotated.X, 6);
            pad.Y = Math.Round(rotated.Y, 6);

            return pad;
        }
    }
}
