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
        public Plain Plain { get; set; }
        public List<Layer> Layers { get; set; }
        public List<Package> Packages { get; set; }
        public List<Component> Components { get; set; }
        public List<Via> Vias { get; set; }

        public double Width { get; set; }
        public double Height { get; set; }

        public List<Fiducial> Fiducials { get; set; }

        public List<Drill> Drills
        {
            get { return Layers.Where(layer => layer.Number == 44).FirstOrDefault().Drills; }
        }

        public List<Hole> Holes
        {
            get { return Layers.Where(layer => layer.Number == 45).FirstOrDefault().Holes; }
        }

        

    }
}
