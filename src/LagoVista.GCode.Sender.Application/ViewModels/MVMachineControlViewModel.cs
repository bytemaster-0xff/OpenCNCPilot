using LagoVista.GCode.Sender.Interfaces;
using LagoVista.GCode.Sender.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public class MVMachineControlViewModel : MachineVisionViewModelBase
    {
        public MVMachineControlViewModel(IMachine machine) : base(machine)
        {
            MachineControlVM = new MachineControlViewModel(Machine);
        }

        public MachineControlViewModel MachineControlVM { get; private set; }
    }
}
