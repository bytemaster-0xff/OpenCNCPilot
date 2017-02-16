using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.EaglePCB.Models
{
    public class PCB
    {
        public List<Drill> Drills { get; private set; }
        public Plain Plain { get; set; }
        public List<Layer> Layers { get; set; }
        public List<Package> Packages { get; set; }
        public List<Component> Components { get; set; }
        public List<Via> Vias { get; set; }
    }
}
