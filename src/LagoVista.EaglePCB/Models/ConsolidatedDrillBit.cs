using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.EaglePCB.Models
{
    public class ConsolidatedDrillBit
    {
        public String NewToolName { get; set; }

        public double Diameter { get; set; }

        public List<DrillBit> Bits { get; set; }
    }
}
