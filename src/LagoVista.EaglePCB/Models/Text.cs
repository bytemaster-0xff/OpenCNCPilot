using System;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class Text
    {
        public int Layer { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string Value { get; set; }
        public double Size { get; set; }

        public static Text Create(XElement element)
        {
            return new Text()
            {
                Layer = element.GetInt32("layer"),
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                Value = element.Value,
                Size = element.GetDouble("size")
            };
        }
    }
}
