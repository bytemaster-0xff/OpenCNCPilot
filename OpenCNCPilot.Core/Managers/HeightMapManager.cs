using LagoVista.GCode.Sender.Interfaces;
using LagoVista.Core.PlatformSupport;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class HeightMapManager : ManagerBase, IHeightMapManager
    {
        IBoardManager _boardManager;

        public HeightMapManager(IMachine machine, ILogger logger, IBoardManager boardManager) : base (machine, logger)
        {
            _boardManager = boardManager;
        }
    }
}
