using System;

namespace LagoVista.Core.GCode.Commands
{
	public abstract class Command 
	{

        public string Line { get; set; }
        public int LineNumber { get; set; }


        public abstract TimeSpan EstimatedRunTime { get; }
    }
}
