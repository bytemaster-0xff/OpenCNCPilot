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
        public int Count { get; set; }

        public int Available { get; set; }

        public string Package { get; set; }
        public string Value { get; set; }

        public string PackageId { get; set; }

        public EntityHeader PartPack { get; set; }
        public string Row { get; set; }
    }
}
