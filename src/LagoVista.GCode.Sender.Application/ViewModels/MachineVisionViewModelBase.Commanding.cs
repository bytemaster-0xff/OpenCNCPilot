using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public abstract partial class MachineVisionViewModelBase
    {
        private bool CanPlay()
        {
            return true;
        }

        private bool CanStop()
        {
            return true;
        }

        public RelayCommand StartCaptureCommand { get; private set; }
        public RelayCommand StopCaptureCommand { get; private set; }
    }
}
