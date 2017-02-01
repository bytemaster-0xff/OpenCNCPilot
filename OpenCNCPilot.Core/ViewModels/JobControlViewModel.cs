using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using LagoVista.Core.ViewModels;
using System;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class JobControlViewModel : GCodeAppViewModel
    {
        public JobControlViewModel(IMachine machine) : base(machine)
        {
            InitCommands();
        }
    }
}
