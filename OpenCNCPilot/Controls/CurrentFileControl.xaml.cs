using LagoVista.GCode.Sender.ViewModels;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace LagoVista.GCode.Sender.Application.Controls
{
    /// <summary>
    /// Interaction logic for CurrentFileStatus.xaml
    /// </summary>
    public partial class CurrentFileControl : UserControl
    {
        public CurrentFileControl()
        {
            InitializeComponent();
        }


        private void ShowGCodeWindow_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var gcodeWindow = new GCodeWindow(DataContext as MainViewModel);

            gcodeWindow.Show();
        }

        private void EditGCode_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
