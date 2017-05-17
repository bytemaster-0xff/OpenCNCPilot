using LagoVista.GCode.Sender.Application.ViewModels;
using LagoVista.GCode.Sender.Interfaces;
using System.Windows;

namespace LagoVista.GCode.Sender.Application.Views
{
    public partial class MVFeederLocatorView : Window
    {
        public MVFeederLocatorView(IMachine machine)
        {
            InitializeComponent();

            var designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!designTime)
            {
                ViewModel = new FeederLocatorViewModel(machine);

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

        public FeederLocatorViewModel ViewModel
        {
            get { return DataContext as FeederLocatorViewModel; }
            set
            {
                DataContext = this;
            }
        }
    }
}

