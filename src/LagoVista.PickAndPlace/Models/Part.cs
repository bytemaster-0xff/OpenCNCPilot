using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Models
{
    public class Part : ModelBase
    {
        public Package Package { get; set; }

        public string Value { get; set; }

        public string PartNumber { get; set; }
    }
}
