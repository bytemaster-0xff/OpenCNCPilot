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

            var elements = from eles
                           in doc.Descendants("element")
                           select new Models.Element()
                           {
                               Name = GetString(eles, "name"),
                               Package = GetString(eles, "package"),
                               X = GetDouble(eles, "x"),
                               Y = GetDouble(eles, "y"),
                               Value = GetString(eles, "value"),
                               Roate = GetString(eles, "rot")
                           };                           

            foreach(var ele in elements)
            {
                Debug.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", ele.Name, ele.Package, ele.X, ele.Y, ele.Value, ele.Roate);
            }


            return pcb;
        }
    }
}
