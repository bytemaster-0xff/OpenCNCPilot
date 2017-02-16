using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class Layer
    {
        public Layer()
        {
            Drills = new List<Drill>();
            Circles = new List<Circle>();
            Holes = new List<Hole>();
            Rects = new List<Rect>();
            Wires = new List<Wire>();
            SMDs = new List<SMD>();
            Pads = new List<Pad>();
            Vias = new List<Via>();
        }

        public int Number { get; set; }
        public string Name { get; set; }
        public int Color { get; set; }
        public int Fill { get; set; }

        public static Layer Create(XElement element)
        {
            return new Layer()
            {
                Number = element.GetInt32("number"),
                Name = element.GetString("name"),
                Color = element.GetInt32("color"),
                Fill = element.GetInt32("fill")
            };
        }

        public List<Drill> Drills { get; set; }

        public List<Circle> Circles { get; set; }
        public List<Hole> Holes { get; set; }
        public List<Rect> Rects { get; set; }

        public List<Wire> Wires { get; set; }
        public List<Via> Vias { get; set; }
        public List<SMD> SMDs { get; set; }
        public List<Pad> Pads { get; set; }

    }


}
