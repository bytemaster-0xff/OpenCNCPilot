using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Models
{
    public class PlaceablePart
    {
        public EntityHeader PartPack { get; set; }
        public string Row { get; set; }
        public int Quantity { get; set; }
    }
}
