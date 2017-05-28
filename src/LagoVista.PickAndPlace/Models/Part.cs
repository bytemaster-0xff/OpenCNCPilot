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
        public int Count { get; set; }
        public Package Package { get; set; }

        public String PackageName { get; set; }

        public String LibraryName { get; set; }

        public string Value { get; set; }

        public double Height { get; set; }

        public string PartNumber { get; set; }

        public String FeederId { get; set; }

        public int? RowNumber { get; set; }

        public string Display
        {
            get
            {
                return $"{PackageName} - {Value}";
            }

        }
    }
}
