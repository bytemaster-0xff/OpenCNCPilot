using LagoVista.Core.Models.Drawing;
using System;

namespace LagoVista.Core.GCode.Commands
{
	public abstract class GCodeCommand 
	{

        public string Line { get; set; }
        public int LineNumber { get; set; }

        public abstract TimeSpan EstimatedRunTime { get; }

        public DateTime? StartTimeStamp { get; set; }

        public abstract Vector3 CurrentPosition { get; }

        public int MessageLength { get { return Line.Length + 1; } }
    }
}
