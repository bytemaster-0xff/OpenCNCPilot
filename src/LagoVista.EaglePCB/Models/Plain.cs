using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class Plain
    {
        public List<Wire> Wires { get; set; }
        public List<Text> Texts { get; set; }

        public static Plain Create(XElement element)
        {
            return new Plain()
            {
                Wires = (from childWires in element.Descendants("wire") select Wire.Create(childWires)).ToList(),
                Texts = (from childTexts in element.Descendants("text") select Text.Create(childTexts)).ToList(),
            };
        }
    }
}
