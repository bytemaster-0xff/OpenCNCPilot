using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        public List<Signal> Signals { get; set; }

        public double Width { get; set; }
        public double Height { get; set; }

        public List<Fiducial> Fiducials { get; set; }

        public List<Drill> Drills
        {
            get
            {
                var drills = Layers.Where(layer => layer.Number == 44).FirstOrDefault().Drills;
                foreach (var via in Vias)
                {
                    var existingDrill = drills.Where(drl => drl.X == via.X && drl.Y == via.Y);

                    /* Vias have drills/holes on top and bottom, only need one */
                    if (!existingDrill.Any())
                    {
                        drills.Add(new Drill() { X = via.X, Y = via.Y, Diameter = via.DrillDiameter });
                    }
                }

                var drillFromHolesLayer = Layers.Where(layer => layer.Number == 45).FirstOrDefault().Drills;
                drills.AddRange(drillFromHolesLayer);

                return drills;
            }
        }

        public List<DrillBit> OriginalToolRack
        {
            get
            {
                /* Probably get this down to about 25% of the lines w/ effective linq...in a hurry KDW 2017-03-15 */
                var drills = Drills.GroupBy(drl => drl.Diameter);
                var bits = new List<DrillBit>();
                var toolIndex = 1;

                foreach (var drill in drills)
                {
                    bits.Add(new DrillBit()
                    {
                        Diameter = drill.First().Diameter,
                    });
                }

                var orderedBits = bits.OrderBy(drl => drl.Diameter).ToList();
                foreach (var bit in orderedBits)
                {
                    bit.ToolName = $"T{toolIndex++:00}";
                }

                return orderedBits;
            }
        }

        public List<Hole> Holes
        {
            get
            {
                foreach(var hole in Layers.Where(layer => layer.Number == 45).FirstOrDefault().Holes)
                {
                    Debug.WriteLine(hole.X + " " + hole.Y + " " + hole.Drill);
                }

                return Layers.Where(layer => layer.Number == 45).FirstOrDefault().Holes;
            }
        }
        public List<Wire> UnroutedWires { get; set; }

        public List<Wire> TopWires { get; set; }

        public List<Wire> BottomWires { get; set; }
    }
}
