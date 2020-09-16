using LagoVista.GCode.Sender.Application.ViewModels;
using LagoVista.PickAndPlace.Models;
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
    /// Interaction logic for PNPJob.xaml
    /// </summary>
    public partial class PNPJobWindow : Window
    {
        public PNPJobWindow()
        {
            InitializeComponent();
        }

        public PnPJobViewModel ViewModel
        {
            get => DataContext as PnPJobViewModel;
        }

        private void WebCamImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var percentX = (e.GetPosition(WebCamImage).X / WebCamImage.ActualWidth) - 0.5;
            var percentY = -((e.GetPosition(WebCamImage).Y / WebCamImage.ActualHeight) - 0.5);
            var absX = percentX * 38 + ViewModel.Machine.MachinePosition.X;
            var absY = percentY * 28 + ViewModel.Machine.MachinePosition.Y;

            ViewModel.Machine.GotoPoint(absX, absY);
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var placeablePart = e.Row.DataContext as PlaceableParts;
            if (placeablePart != null && placeablePart.Row != null)
            {
                if (placeablePart.Row.AvailableParts >= placeablePart.Count)
                {
                    e.Row.Background = new SolidColorBrush(Colors.Green);
                    e.Row.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    e.Row.Background = new SolidColorBrush(Colors.Yellow);
                }
            }
        }
    }
}
