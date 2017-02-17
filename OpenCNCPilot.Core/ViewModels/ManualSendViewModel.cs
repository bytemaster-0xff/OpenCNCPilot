
using LagoVista.GCode.Sender.Interfaces;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class ManualSendViewModel : GCodeAppViewModelBase
    {

        public ManualSendViewModel(IMachine machine) :base(machine)
        {
            InitCommands();
        }
    }
}
