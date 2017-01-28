using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCNCPilot.Core.Communication
{
    public partial class Machine
    {
        public enum OperatingMode
        {
            Manual,
            SendFile,
            Probe,
            Disconnected
        }

    }
}
