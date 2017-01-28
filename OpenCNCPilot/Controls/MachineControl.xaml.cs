using OpenCNCPilot.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenCNCPilot.Controls
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
            DataContext = new MachineControlViewModel(App.Current.Machine, App.Current.Settings);
        }
    }
}
