using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class Component
    {
        public string Name { get; set; }
        public string LibraryName { get; set; }
        public string Package { get; set; }
        public string Value { get; set; }
        public double? X { get; set; }
        public double? Y { get; set; }
        public string Roate { get; set; }

        public static Component Create(XElement element)
        {
            return new Component()
            {
                Name = element.GetString("name"),
                LibraryName = element.GetString("library"),
                Package = element.GetString("package"),
                Value = element.GetString("value"),
                X = element.GetDouble("x"),
                Y = element.GetDouble("y")
            };
        }
    }
}
