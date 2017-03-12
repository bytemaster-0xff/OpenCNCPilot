using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MachineVisionViewModel : GCodeAppViewModelBase
    {
        public MachineVisionViewModel(IMachine machine) :base(machine)
        {


        }
    }
}
