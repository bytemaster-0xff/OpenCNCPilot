

using LagoVista.GCode.Sender.Interfaces;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MachineControlViewModel : GCodeAppViewModelBase
    {
        public MachineControlViewModel(IMachine machine) : base(machine)
        {
            InitCommands();
        }      
    }
}
