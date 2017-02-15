using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class Rect
    {
        public double X1 { get; set; }
        public double X2 { get; set; }
        public double Y1 { get; set; }
        public double Y2 { get; set; }

        public static Rect Create(XElement element)
        {
            return new Rect()
            {
                X1 = element.GetDouble("x1"),
                X2 = element.GetDouble("x2"),
                Y1 = element.GetDouble("y1"),
                Y2 = element.GetDouble("y2"),
            };
        }
    }
}
