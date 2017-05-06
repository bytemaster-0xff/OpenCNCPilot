using LagoVista.GCode.Sender.ViewModels;
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
using System.Windows.Shapes;

namespace LagoVista.GCode.Sender.Application
{
    /// <summary>
    /// Interaction logic for GCodeWindow.xaml
    /// </summary>
    public partial class GCodeWindow : Window
    {
        public GCodeWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();

            DataContext = mainViewModel;

            CurrentFile.ShowGCodeWindow.Visibility = Visibility.Collapsed;
        }
    }
}
