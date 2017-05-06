using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class NewHeightMapViewModel
    {
        private void InitCommands()
        {
            GenerateTestPatternCommand = new Core.Commanding.RelayCommand(GenerateTestPattern);
        }

        public Core.Commanding.RelayCommand GenerateTestPatternCommand { get; private set; }
    }
}
