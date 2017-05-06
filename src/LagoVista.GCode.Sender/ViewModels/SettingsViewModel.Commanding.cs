using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class SettingsViewModel
    {
        private void InitComamnds()
        {
            CancelCommand = new RelayCommand(Cancel);
        }

        public bool CanChangeMachineConfig
        {
            get { return !_machine.Connected; }
        }


        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
    }
}
