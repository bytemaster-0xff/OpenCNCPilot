using LagoVista.Core.PlatformSupport;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class ToolChangeManager
    {
        public ToolChangeManager(IMachine machine, ILogger logger)
        {
            Machine = machine;
            Logger = logger;
        }
    }
}
