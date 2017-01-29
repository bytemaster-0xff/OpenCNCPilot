
using LagoVista.GCode.Sender.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace LagoVista.GCode.Sender.Application.Controls
{
    /// <summary>
    /// Interaction logic for MachineControl.xaml
    /// </summary>
    public partial class MachineControl : UserControl
    {
        public MachineControl()
        {
            InitializeComponent();
            Loaded += MachineControl_Loaded;
        }

        private void MachineControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new MachineControlViewModel(App.Current.Machine, App.Current.Settings);
            }
        }
    }
}
