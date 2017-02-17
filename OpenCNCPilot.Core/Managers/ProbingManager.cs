using LagoVista.Core.PlatformSupport;
using LagoVista.GCode.Sender.Interfaces;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class ProbingManager : ManagerBase, IProbingManager
    {
        public ProbingManager(IMachine machine, ILogger logger) : base(machine, logger)
        {

        }
    }
}
