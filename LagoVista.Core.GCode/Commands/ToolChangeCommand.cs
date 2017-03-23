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
        public override Vector3 CurrentPosition { get; set; }
       
        public override TimeSpan EstimatedRunTime
        {
            get { return TimeSpan.Zero; }
        }

        public string ToolName { get; set; }

        public string ToolSize { get; set; }

        public override string ToString()
        {
            return $"{LineNumber}. - {OriginalLine} Set Tool: {ToolName}, ToolSize: {ToolSize}";
        }

        public override void SetComment(string comment)
        {
            ToolSize = comment.Trim();
        }
    }
}
