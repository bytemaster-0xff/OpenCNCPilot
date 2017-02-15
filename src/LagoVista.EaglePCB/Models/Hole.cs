using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class Hole
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Drill { get; set; }

        public static Hole Create(XElement element)
        {
            return new Models.Hole()
            {
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                Drill = element.GetDouble("drill")
            };
        }
    }
}
