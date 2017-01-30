using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class NewHeightMapViewModel : GCodeAppViewModel
    {
        public NewHeightMapViewModel(IMachine machine, Settings settings) : base(machine, settings)
        {
            InitCommands();
        }
    }
}
