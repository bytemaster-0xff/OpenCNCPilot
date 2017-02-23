using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class Wire
    {
        public string Name { get; set; }
        public double Width { get; set; }
        public double? Curve { get; set; }
        public Rect Rect { get; set; }

        public int Layer { get; set; }

        public static Wire Create(XElement element)
        {
            return new Wire()
            {
                Name = element.GetString("name"),
                Rect = Rect.Create(element),
                Width = element.GetDouble("width"),
                Layer = element.GetInt32("layer"),
                Curve = element.GetDoubleNullable("curve")
            };
        }

        public override string ToString()
        {
            return $"Wire => X1={Rect.X1}, Y1={Rect.Y1}, X2={Rect.X2}, Y2={Rect.Y2}, Width={Width}, Curve={Curve}";
        }
    }
}
