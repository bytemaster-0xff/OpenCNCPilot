using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class NewHeightMapViewModel : GCodeAppViewModelBase
    {
      
        public NewHeightMapViewModel(IMachine machine) : base(machine)
        {
            InitCommands();
        }
    }
}
