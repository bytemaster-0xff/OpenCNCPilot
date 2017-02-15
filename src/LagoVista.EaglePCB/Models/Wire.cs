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
        public Rect Rect { get; set; }

        public int Layer { get; set; }

        public static Wire Create(XElement element)
        {
            return new Wire()
            {
                Name = element.GetString("name"),
                Rect = Rect.Create(element),
                Width = element.GetDouble("width"),
                Layer = element.GetInt32("layer")
            };
        }
    }
}
