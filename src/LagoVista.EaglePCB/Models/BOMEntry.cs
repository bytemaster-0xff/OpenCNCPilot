using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.EaglePCB.Models
{
    public class BOMEntry
    {
        public BOMEntry()
        {
            Components = new List<Component>();
        }

        public Package Package { get;  internal set; }

        public string Value { get; internal set; }

        public List<Component> Components { get; private set; }

        public override string ToString()
        {
            return $"{Package.Name} - {Value}, QTY: {Components.Count}";
        }
    }
}
