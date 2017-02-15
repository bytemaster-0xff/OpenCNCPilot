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

            return pcb;
        }
    }
}
