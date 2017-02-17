using LagoVista.GCode.Sender.Interfaces;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class JobControlViewModel : GCodeAppViewModelBase
    {
        public JobControlViewModel(IMachine machine) : base(machine)
        {
            InitCommands();
        }
    }
}
