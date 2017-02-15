using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class Circle
    {
        public int Layer { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Radius { get; set; }
        public double Width { get; set; }

        public Package Package { get; set; }

        public static Circle Create(XElement element)
        {
            return new Circle()
            {
                Layer = element.GetInt32("layer"),
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                Radius = element.GetDouble("radius"),
                Width = element.GetDouble("width"),
            };
        }
    }
}
