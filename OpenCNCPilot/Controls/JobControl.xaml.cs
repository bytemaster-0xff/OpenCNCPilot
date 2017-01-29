using OpenCNCPilot.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for JobControl.xaml
    /// </summary>
    public partial class JobControl : UserControl
    {
        public JobControl()
        {
            InitializeComponent();

            this.Loaded += JobControl_Loaded;
        }

        private void JobControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new JobControlViewModel(App.Current.Machine, App.Current.Settings);
            }
        }
    }
}
