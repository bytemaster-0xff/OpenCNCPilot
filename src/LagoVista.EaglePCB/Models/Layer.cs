using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class Layer
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public int Color { get; set; }
        public int Fill { get; set; }

        public static Layer Create(XElement element)
        {
            return new Layer()
            {
                Number = element.GetInt32("number"),
                Name = element.GetString("name"),
                Color = element.GetInt32("color"),
                Fill = element.GetInt32("fill")
            };
        }
    }


}
