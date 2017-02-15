using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class Package
    {
        public string LibraryName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<Wire> Wires { get; set; }
        public List<SMD> SMDs { get; set; }
        public List<Text> Texts { get; set; }
        public List<Pad> Pads { get; set; }
        public List<Circle> Circles { get; set; }
        public List<Hole> Holes { get; set; }
        public List<Rect> Rects { get; set; }

        public static Package Create(XElement element)
        {
            return new Package()
            {
                LibraryName = element.Ancestors(XName.Get("library")).First().Attribute("name").Value,
                Name = element.GetString("name"),
                Description = element.GetChildString("description"),
                Wires = (from childWires in element.Descendants("wire") select Wire.Create(childWires)).ToList(),
                Texts = (from childTexts in element.Descendants("text") select Text.Create(childTexts)).ToList(),
                SMDs = (from childSMDs in element.Descendants("smd") select SMD.Create(childSMDs)).ToList(),
                Pads = (from childPads in element.Descendants("pad") select Pad.Create(childPads)).ToList(),
                Holes = (from childPads in element.Descendants("hole") select Hole.Create(childPads)).ToList(),
                Circles = (from childCircles in element.Descendants("circle") select Circle.Create(childCircles)).ToList(),
                Rects = (from childCircles in element.Descendants("rect") select Rect.Create(childCircles)).ToList(),
            };
        }

    }
}
