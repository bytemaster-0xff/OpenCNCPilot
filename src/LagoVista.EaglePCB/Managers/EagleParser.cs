using System;
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
        private static string GetString (XElement element, string name)
        {
            if(element.Attributes(XName.Get(name)).Any())
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

            foreach(var layer in pcb.Layers)
            {
                foreach(var element in pcb.Components)
                {
                    var package = pcb.Packages.Where(pkg => pkg.LibraryName == element.LibraryName && pkg.Name == element.PackageName).FirstOrDefault();
                    layer.SMDs = new List<Models.SMD>();
                    foreach(var smd in package.SMDs.Where(smd => smd.Layer == layer.Number))
                    {
                        smd.Package = package;
                        layer.SMDs.Add(smd);
                    }

                    layer.Circles = new List<Models.Circle>();
                    foreach (var circle in package.Circles.Where(smd => smd.Layer == layer.Number))
                    {
                        circle.Package = package;
                        layer.Circles.Add(circle);
                    }
                }
            }

            return pcb;
        }
    }
}
