using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LagoVista.Core.Models.Drawing;

namespace LagoVista.Core.GCode.Commands
{
    public class GCodeProbe : GCodeCommand
    {
        public override TimeSpan EstimatedRunTime { get { return TimeSpan.FromSeconds(5); } }

        public override Vector3 CurrentPosition { get; set; }

        public double Feed { get; set; }

        public override string ToString()
        {
            return $"Probe Command {Command}";
        }
    }
}
