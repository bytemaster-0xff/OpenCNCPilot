using System;
using System.Linq;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Managers
{
    public class EagleParser
    {

        public static Models.PCB ReadPCB(XDocument doc)
        {
            var pcb = new Models.PCB();

            pcb.Components = (from eles
                           in doc.Descendants("element")
                              select Models.Component.Create(eles)).ToList();

            pcb.Layers = (from eles
                           in doc.Descendants("layer")
                          select Models.Layer.Create(eles)).ToList();

            pcb.Packages = (from eles
                           in doc.Descendants("package")
                            select Models.Package.Create(eles)).ToList();

            pcb.Plain = (from eles
                         in doc.Descendants("plain")
                         select Models.Plain.Create(eles)).First();

            pcb.Vias = (from eles
                        in doc.Descendants("via")
                        select Models.Via.Create(eles)).ToList();

            pcb.Signals = (from eles
                        in doc.Descendants("signal")
                           select Models.Signal.Create(eles)).ToList();

            /* FIrst assign packages to components */
            foreach (var element in pcb.Components)
            {
                element.Package = pcb.Packages.Where(pkg => pkg.LibraryName == element.LibraryName && pkg.Name == element.PackageName).FirstOrDefault();

                foreach (var layer in pcb.Layers)
                {
                    layer.Wires = pcb.Plain.Wires.Where(wire => wire.Layer == layer.Number).ToList();
                    if (layer.Number == 17)
                    {
                        foreach (var pad in element.Package.Pads)
                        {
                            var rotatedPad = pad.ApplyRotation(element.Rotate.ToAngle());
                            layer.Pads.Add(new Models.Pad() { DrillDiameter = rotatedPad.DrillDiameter, X = element.X.Value + rotatedPad.X, Y = element.Y.Value + rotatedPad.Y, RotateStr = pad.RotateStr });
                        }
                    }

                    if (layer.Number == 44)
                    {
                        foreach (var hole in element.Package.Pads)
                        {
                            var rotatedHole = hole.ApplyRotation(element.Rotate.ToAngle());
                            layer.Drills.Add(new Models.Drill() { Diameter = hole.DrillDiameter, X = element.X.Value + rotatedHole.X, Y = element.Y.Value + rotatedHole.Y, Name=hole.Name });
                        }
                    }

                    if (layer.Number == 45)
                    {
                        //foreach (var hole in element.Package.Holes)
                        foreach(var hole in element.Holes)
                        {
                            layer.Holes.Add(new Models.Hole() { Drill = hole.Drill, X = hole.X, Y = hole.Y, Name=element.Name });
                            layer.Drills.Add(new Models.Drill() { Diameter = hole.Drill, X = hole.X, Y = hole.Y, Name = element.Name });
                        }
                    }
                }
            }

            pcb.UnroutedWires = new System.Collections.Generic.List<Models.Wire>();
            pcb.TopWires = new System.Collections.Generic.List<Models.Wire>();
            pcb.BottomWires = new System.Collections.Generic.List<Models.Wire>();

            foreach(var signal in pcb.Signals)
            {
                pcb.UnroutedWires.AddRange(signal.UnroutedWires);
                pcb.TopWires.AddRange(signal.TopWires);
                pcb.BottomWires.AddRange(signal.BottomWires);
            }

            var outlineWires = pcb.Layers.Where(layer => layer.Number == 20).FirstOrDefault().Wires;

            foreach (var outline in outlineWires)
            {
                pcb.Width = Math.Max(outline.Rect.X1, pcb.Width);
                pcb.Width = Math.Max(outline.Rect.X2, pcb.Width);
                pcb.Height = Math.Max(outline.Rect.Y1, pcb.Height);
                pcb.Height = Math.Max(outline.Rect.Y2, pcb.Height);
            }

            foreach (var via in pcb.Vias)
            {
                pcb.Layers.Where(layer => layer.Number == 18).First().Vias.Add(via);
            }

            return pcb;
        }
    }
}
