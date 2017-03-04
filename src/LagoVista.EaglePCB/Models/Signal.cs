using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class Signal
    {
        public List<ContactRef> Contacts { get; private set; }
        public List<Wire> Wires { get; private set; }

        public static Signal Create(XElement element)
        {
            return new Signal()
            {
                Wires = (from childWires in element.Descendants("wire") select Wire.Create(childWires)).ToList(),
                Contacts = (from refs in element.Descendants("contactref") select ContactRef.Create(refs)).ToList(),
            };
        }

        public List<Wire> UnroutedWires
        {
            get { return Wires.Where(wire => wire.Layer == 19).ToList(); }
        }

        public List<Wire> TopWires
        {
            get { return Wires.Where(wire => wire.Layer == 1).ToList(); }
        }

        public List<Wire> BottomWires
        {
            get { return Wires.Where(wire => wire.Layer == 16).ToList(); }
        }
    }
}
