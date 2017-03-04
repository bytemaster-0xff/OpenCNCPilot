using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class Via
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double OriginX { get; set; }
        public double OriginY { get; set; }
        public double DrillDiameter { get; set; }

        public static Via Create(XElement element)
        {
            return new Via()
            {
                DrillDiameter = element.GetDouble("drill"),
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                OriginX = element.GetDouble("x"),
                OriginY = element.GetDouble("y")
            };
        }
    }
}
