using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Core.GCode.Commands
{
    public class MCode : Command
	{
		public int Code;

        public override string ToString()
        {
            return $"{LineNumber}. - {Line}";
        }

        public override TimeSpan EstimatedRunTime
        {
            get
            {
                return TimeSpan.Zero;
            }
        }
    }
}
