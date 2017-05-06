using LagoVista.Core.PlatformSupport;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class MachineVisionManager : ManagerBase, IMachineVisionManager
    {
        IPCBManager _boardManager;
        public MachineVisionManager(IMachine machine, ILogger logger, IPCBManager boardManager) : base(machine, logger)
        {
            _boardManager = boardManager;
        }
    }
}
