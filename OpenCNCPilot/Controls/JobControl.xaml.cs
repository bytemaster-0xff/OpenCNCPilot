using LagoVista.GCode.Sender.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace LagoVista.GCode.Sender.Application.Controls
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
