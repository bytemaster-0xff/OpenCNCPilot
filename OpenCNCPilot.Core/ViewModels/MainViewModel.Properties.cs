using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MainViewModel
    {
        HeightMapViewModel _heightMapVieModel;
        public HeightMapViewModel HeightMapVM
        {
            get { return _heightMapVieModel; }
            set { Set(ref _heightMapVieModel, value); }
        }

        JobControlViewModel _jobControlVM;
        public JobControlViewModel JobControlVM
        {
            get { return _jobControlVM; }
            set { Set(ref _jobControlVM, value); }
        }

        MachineControlViewModel _machineControlVM;
        public MachineControlViewModel MachineControlVM
        {
            get { return _machineControlVM; }
            set { Set(ref _machineControlVM, value); }
        }

        private ManualSendViewModel _manualSendVM;
        public ManualSendViewModel ManualSendVM
        {
            get { return _manualSendVM; }
            set { Set(ref _manualSendVM, value); }
        }
    }
}
