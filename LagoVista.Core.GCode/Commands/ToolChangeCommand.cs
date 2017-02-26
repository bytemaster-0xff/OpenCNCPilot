using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LagoVista.Core.Models.Drawing;

namespace LagoVista.Core.GCode.Commands
{
    public class ToolChangeCommand : GCodeCommand
    {
        public override Vector3 CurrentPosition
        {
            get { return new Vector3(0, 0, 0); }
        }

        public override TimeSpan EstimatedRunTime
        {
            get { return TimeSpan.Zero; }
        }

        public string ToolName { get; set; }

        public string ToolSize { get; set; }

        public override string ToString()
        {
            return $"{LineNumber}. - {Line} Set Tool: {ToolName}, ToolSize: {ToolSize}";
        }
    }
}
