using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.EaglePCB.Models
{
    public enum FidicualTypes
    {
        Circle,
        BoardEdge,
    }

    public class Fiducial
    {
        public double X { get; set; }
        public double Y { get; set; }

        public double Diameter { get; set; }

        public FidicualTypes FiducialType { get; set; }
    }
}
