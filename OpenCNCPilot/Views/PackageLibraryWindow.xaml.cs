using LagoVista.PickAndPlace.ViewModels;
using System.Windows;

namespace LagoVista.GCode.Sender.Application.Views
{
    /// <summary>
    /// Interaction logic for PackageLibraryWindow.xaml
    /// </summary>
    public partial class PackageLibraryWindow : Window
    {
        public PackageLibraryWindow()
        {
            InitializeComponent();
            DataContext = new PackageLibraryViewModel();
        }
    }
}
