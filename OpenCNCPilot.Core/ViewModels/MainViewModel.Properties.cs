using LagoVista.GCode.Sender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MainViewModel
    {
        public JobControlViewModel JobControlVM { get; private set; }
        public MachineControlViewModel MachineControlVM { get; private set; }
        public ManualSendViewModel ManualSendVM { get; private set; }
    }
}
