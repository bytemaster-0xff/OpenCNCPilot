using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LagoVista.Core.Models.Drawing;

namespace LagoVista.Core.GCode.Commands
{
    public class GCodeDwell : GCodeCommand
    {
        public TimeSpan DwellTime { get; set; }

        public override TimeSpan EstimatedRunTime
        {
            get { return DwellTime; }
        }

        public override Vector3 CurrentPosition { get; set; }
    }
}
