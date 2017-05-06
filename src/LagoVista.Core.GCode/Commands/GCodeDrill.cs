using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LagoVista.Core.Models.Drawing;

namespace LagoVista.Core.GCode.Commands
{
    public class GCodeDrill : GCodeMotion
    {
        public override double Length
        {
            get
            {
                return (End.Z - Start.Z) * 2;
            }
        }

        public override Vector3 Interpolate(double ratio)
        {
            if(ratio < 0.5)
            {
                return new Vector3(Start.X, Start.Y, Start.Z + (End.Z - Start.Z) * ratio);
            }
            else
            {
                return new Vector3(Start.X, Start.Y, Start.Z + (End.Z - Start.Z) * 0.5 - ratio);
            }
        }

        public override IEnumerable<GCodeMotion> Split(double length)
        {
            return new List<GCodeMotion>() { this };
        }


    }
}
