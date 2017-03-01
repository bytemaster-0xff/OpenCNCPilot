using LagoVista.Core.PlatformSupport;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class PCBManager : Core.Models.ModelBase, IPCBManager
    {
        IMachine _machine;
        ILogger _logger;

        public PCBManager(IMachine machine, ILogger logger)
        {
            _logger = logger;
            _machine = machine;
        }
    }
}
