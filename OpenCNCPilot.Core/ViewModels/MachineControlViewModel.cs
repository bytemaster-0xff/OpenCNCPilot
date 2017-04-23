

using LagoVista.GCode.Sender.Interfaces;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MachineControlViewModel : GCodeAppViewModelBase
    {
        public MachineControlViewModel(IMachine machine) : base(machine)
        {
            InitCommands();
            if(machine.Settings != null)
            {
                XYStepMode = Machine.Settings.XYStepMode;
                ZStepMode = Machine.Settings.ZStepMode;
            }
        }      
    }
}
