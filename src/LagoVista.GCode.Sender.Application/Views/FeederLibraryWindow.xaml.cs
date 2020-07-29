using LagoVista.PickAndPlace.ViewModels;
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

namespace LagoVista.GCode.Sender.Application.Views
{
    /// <summary>
    /// Interaction logic for FeederLibraryWindow.xaml
    /// </summary>
    public partial class FeederLibraryWindow : Window
    {
        public FeederLibraryWindow()
        {
            InitializeComponent();
            this.Loaded += FeederLibraryWindow_Loaded;
            //DataContext = new FeederDefinitionsViewModel();
        }

        private async void FeederLibraryWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await (DataContext as FeederDefinitionsViewModel).InitAsync();
        }
    }
}
