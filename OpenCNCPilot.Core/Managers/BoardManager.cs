using LagoVista.GCode.Sender.Interfaces;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using LagoVista.Core.PlatformSupport;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class BoardManager : IBoardManager
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public BoardManager(IMachine machine, ILogger logger, IHeightMapManager heightMapManager)
        {
            Machine = machine;
            Logger = logger;
            HeightMapManager = heightMapManager;
        }
    }
}