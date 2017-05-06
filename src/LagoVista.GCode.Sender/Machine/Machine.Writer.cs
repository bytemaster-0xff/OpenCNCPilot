using LagoVista.Core.GCode.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender
{
    public partial class Machine
    {
        public void SendCommand(GCodeCommand cmd)
        {
            if (AssertConnected())
            {
                Enqueue(cmd);
            }
        }

        public void SendCommand(String cmd)
        {
            if (AssertConnected())
            {
                Enqueue(cmd);
            }
        }
    }
}
