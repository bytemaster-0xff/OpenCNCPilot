

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MachineControlViewModel : GCodeAppViewModel
    {
        public MachineControlViewModel(IMachine machine, Settings settings) : base(machine, settings)
        {
            XYStep = settings.MediumStepSize;
            ZStep = settings.MediumStepSize;

            InitCommands();
        }
    }
}
