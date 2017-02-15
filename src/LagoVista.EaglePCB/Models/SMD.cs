using System;
using System.Diagnostics;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class SMD
    {
        public int Layer { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double DX { get; set; }
        public double DY { get; set; }
        public double? Roundness { get; set; }
        public string RotateStr { get; set; }

        public Package Package { get; set; }

        public static SMD Create(XElement element)
        {
            return new SMD()
            {
                Layer = element.GetInt32("layer"),
                Name = element.GetString("name"),
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                DX = element.GetDouble("dx"),
                DY = element.GetDouble("dy"),
                Roundness = element.GetDoubleNullable("roundness"),
                RotateStr = element.GetString("rot")
            };
        }
    }
}
