using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LagoVista.Core;
using System.Threading.Tasks;
using LagoVista.Core.Models;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Defines the Physical Definition of a Parts Tray
    /// </summary>
    public class Feeder : ModelBase
    {
        public string Id { get; set; }

        public Feeder()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Name { get; set; }

        public double Width { get; set; }
        public double Length{ get; set; }
        public double PartZ { get; set; }

        public double TapeWidth { get; set; }

        public double RowWidth { get; set; }

        public double FirstRowOffset { get; set; }

        public bool IsStatic { get; set; }

        public int NumberRows { get; set; }

        private double _x;
        private double _y;
        public double X 
        { 
            get { return _x; }
            set
            {
                Set(ref _x, value);
            }
        }
        public double Y
        {
            get { return _y; }
            set
            {
                Set(ref _y, value);
            }
        }

        public override string ToString()
        {
            return $"{Name} - {X} {Y}";
        }
    }
}
