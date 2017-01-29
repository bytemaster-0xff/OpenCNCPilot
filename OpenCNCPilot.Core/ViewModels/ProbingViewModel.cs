using LagoVista.Core.ViewModels;
using LagoVista.GCode.Sender.Models;

namespace LagoVista.GCode.Sender.ViewModels
{
    public class ProbingViewModel : ViewModelBase
    {
        private PCBoard _board;
        IMachine _machine;
        public ProbingViewModel(IMachine machine)
        {
            _machine = machine;
        }

        public PCBoard Board
        {
            get { return _board; }
            set { Set(ref _board, value); }
        }
    }
}
