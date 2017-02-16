﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Managers
{
    public class EagleParser
    {
        private static string GetString(XElement element, string name)
        {
            if (element.Attributes(XName.Get(name)).Any())
            {
                return element.Attribute(XName.Get(name)).Value;
            }

            return String.Empty;
        }

        public static double? GetDouble(XElement element, string name)
        {
            if (element.Attributes(XName.Get(name)).Any())
            {
                return Convert.ToDouble(element.Attribute(XName.Get(name)).Value);
            }

            return null;
        }

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
                            layer.Pads.Add(new Models.Pad() { Drill = pad.Drill, X = element.X.Value + pad.X, Y = element.Y.Value + pad.Y, RotateStr = pad.RotateStr });
                        }
                    }

                    if (layer.Number == 44)
                    {
                        foreach (var hole in element.Package.Pads)
                        {
                            layer.Drills.Add(new Models.Drill() { Diameter = hole.Drill, X = element.X.Value + hole.X, Y = element.Y.Value });
                        }
                    }

                    if (layer.Number == 45)
                    {
                        foreach (var hole in element.Package.Holes)
                        {
                            layer.Holes.Add(new Models.Hole() { Drill = hole.Drill, X = element.X.Value + hole.X, Y = element.Y.Value });
                        }
                    }
                }
            }

            foreach (var via in pcb.Vias)
            {
                pcb.Layers.Where(layer => layer.Number == 18).First().Vias.Add(via);
            }

            return pcb;
        }
    }
}