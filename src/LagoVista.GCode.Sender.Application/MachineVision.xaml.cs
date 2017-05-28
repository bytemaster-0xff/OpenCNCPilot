using LagoVista.GCode.Sender.ViewModels;
using System;
using System.Windows;
using Emgu.CV;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media.Imaging;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.GCode.Sender.Application.ViewModels;

namespace LagoVista.GCode.Sender.Application
{
    /// <summary>
    /// Interaction logic for MachineVision.xaml
    /// </summary>
    public partial class MachineVision : Window
    {
        //MachineVisionViewModel _viewModel;

        public MachineVision(IMachine machine)
        {
            InitializeComponent();

            var designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!designTime)
            {
          //      ViewModel = new MachineVisionViewModel(machine);

                this.Closing += MachineVision_Closing;
                this.Loaded += MachineVision_Loaded;
            }
        }

        private void MachineVision_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //ViewModel.StopCapture();
        }

        private void MachineVision_Loaded(object sender, RoutedEventArgs e)
        {
            //await ViewModel.InitAsync();
        }
      
        /*
        public MachineVisionViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
                DataContext = this;
            }
        }*/
    }
}
