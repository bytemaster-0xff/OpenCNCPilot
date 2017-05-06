using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LagoViata.PNP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Drivers.StepperDriver _stepperDriver;

        public MainPage()
        {
            this.InitializeComponent();

            _stepperDriver = new Drivers.StepperDriver();

            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await _stepperDriver.InitAsync();
        }

        private async void Forard_Click(object sender, RoutedEventArgs e)
        {
            _stepperDriver.Enable();
            await _stepperDriver.YAxis.Step(80, Drivers.Direction.Forward);
            //_stepperDriver.Disable();
        }

        private async  void Backwards_Click(object sender, RoutedEventArgs e)
        {
            _stepperDriver.Enable();
            await _stepperDriver.YAxis.Step(80, Drivers.Direction.Backwards);
            //_stepperDriver.Disable();
        }
    }
}
