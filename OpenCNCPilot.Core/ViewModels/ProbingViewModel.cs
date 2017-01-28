using LagoVista.Core.ViewModels;
using OpenCNCPilot.Core.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCNCPilot.Core.ViewModels
{
    public class ProbingViewModel : ViewModelBase
    {
        private Models.Board _board;
        IMachine _machine;
        public ProbingViewModel(IMachine machine)
        {
            _machine = machine;
        }

        public Models.Board Board
        {
            get { return _board; }
            set { Set(ref _board, value); }
        }
    }
}
