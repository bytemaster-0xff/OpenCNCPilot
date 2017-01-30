using LagoVista.Core.ViewModels;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MainViewModel : GCodeAppViewModel
    {
        public MainViewModel(IMachine machine, Settings settings) : base(machine, settings)
        {
            InitCommands();
            InitChildViewModels();
        }

        private void InitChildViewModels()
        {
            HeightMapVM = new HeightMapViewModel(Machine, Settings);
            JobControlVM = new JobControlViewModel(Machine, Settings);
            ManualSendVM = new ManualSendViewModel(Machine, Settings);
            MachineControlVM = new MachineControlViewModel(Machine, Settings);
        }
    }
}
