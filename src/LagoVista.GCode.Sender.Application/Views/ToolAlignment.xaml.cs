using LagoVista.GCode.Sender.Application.ViewModels;
using LagoVista.GCode.Sender.Interfaces;
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
    /// Interaction logic for ToolAlignment.xaml
    /// </summary>
    public partial class ToolAlignment : Window
    {
        public ToolAlignment(IMachine machine)
        {
            InitializeComponent();
            var designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!designTime)
            {
                ViewModel = new ViewModels.ToolAlignmentViewModel(machine);

                this.Closing += MachineVision_Closing;
                this.Loaded += MachineVision_Loaded;
            }
        }

        private void MachineVision_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModel.StopCapture();
        }

        private async void MachineVision_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitAsync();
        }

        public ToolAlignmentViewModel ViewModel
        {
            get { return DataContext as ToolAlignmentViewModel; }
            set
            {
                DataContext = value;
            }
        }

        private void WebCamImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var percentX = (e.GetPosition(WebCamImage).X / WebCamImage.ActualWidth) - 0.5;
            var percentY = -((e.GetPosition(WebCamImage).Y / WebCamImage.ActualHeight) - 0.5);
            var absX = percentX * 38 + ViewModel.Machine.MachinePosition.X;
            var absY = percentY * 28 + ViewModel.Machine.MachinePosition.Y;

            ViewModel.Machine.GotoPoint(absX, absY);
        }

        private void BirdsEye_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var absX = (e.GetPosition(BirdsEye).X) * 2.0;
            var absY = (BirdsEye.ActualHeight - e.GetPosition(BirdsEye).Y) * 2.0;

            ViewModel.Machine.GotoPoint(absX, absY);
        }

    }
}
