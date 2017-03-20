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
        GCodeWindow _gCodeWindow;
        public CurrentFileControl()
        {
            InitializeComponent();
        }


        private void ShowGCodeWindow_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_gCodeWindow == null)
            {
                _gCodeWindow = new GCodeWindow(DataContext as MainViewModel);
                _gCodeWindow.Owner = MainWindow.This;
                _gCodeWindow.Closing += _window_Closing;
                _gCodeWindow.Show();
            }
        }

        private void _window_Closing(object sender, CancelEventArgs e)
        {
            _gCodeWindow = null;
        }

        private void EditGCode_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
