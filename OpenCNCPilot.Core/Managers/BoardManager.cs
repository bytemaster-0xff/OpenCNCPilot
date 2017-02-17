using LagoVista.GCode.Sender.Interfaces;
using LagoVista.Core.PlatformSupport;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class BoardManager : ManagerBase, IBoardManager
    {

        public BoardManager(IMachine machine, ILogger logger) : base(machine, logger)
        {
        }
    }
}