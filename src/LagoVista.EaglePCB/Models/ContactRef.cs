using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Models
{
    public class ContactRef
    {
        public string ElementName { get; set; }
        public string Pad { get; set; }

        public static ContactRef Create(XElement element)
        {
            return new ContactRef()
            {
                ElementName = element.GetString("element"),
                Pad = element.GetString("pad"),
            };
        }
    }
}
