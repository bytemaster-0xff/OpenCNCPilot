using LagoVista.GCode.Sender.ViewModels;
using System.Windows.Controls;

namespace LagoVista.GCode.Sender.Application.Controls
{
    /// <summary>
    /// Interaction logic for MachineResponse.xaml
    /// </summary>
    public partial class MachineResponseControl : UserControl
    {
        MessageWindow _messagesWindow;
        public MachineResponseControl()
        {
            InitializeComponent();
        }

        private void ShowLogWindow_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_messagesWindow == null)
            {
                var vm = DataContext as MainViewModel;
                _messagesWindow = new MessageWindow(vm.Machine);
                _messagesWindow.Owner = MainWindow.This;
                _messagesWindow.Closed += _messagesWindow_Closed;
                _messagesWindow.Show();
            }

        }

        private void _messagesWindow_Closed(object sender, System.EventArgs e)
        {
            _messagesWindow = null;
        }
    }
}
