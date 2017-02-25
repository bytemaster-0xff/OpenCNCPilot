using LagoVista.GCode.Sender.ViewModels;
using System.Windows.Controls;

namespace LagoVista.GCode.Sender.Application.Controls
{
    /// <summary>
    /// Interaction logic for MachineResponse.xaml
    /// </summary>
    public partial class MachineResponseControl : UserControl
    {
        public MachineResponseControl()
        {
            InitializeComponent();
        }

        private void ShowLogWindow_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            var msgsWindow = new MessageWindow(vm.Machine);
            msgsWindow.Show();

        }
    }
}
