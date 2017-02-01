
namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class ManualSendViewModel : GCodeAppViewModel
    {

        public ManualSendViewModel(IMachine machine) :base(machine)
        {
            InitCommands();
        }
    }
}
