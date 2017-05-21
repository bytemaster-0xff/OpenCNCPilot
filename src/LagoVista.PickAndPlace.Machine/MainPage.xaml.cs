using LagoViata.PNP.Services;
using LagoVista.Core.GCode.Parser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.AppService;
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
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        Machine _machine;

        AppServiceConnection _appServiceConnection;
        DispatcherTimer _timer;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            _startup = DateTime.Now.ToString();

            this.InitializeComponent();

            this.Loaded += MainPage_Loaded;

            this.DataContext = this;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(250);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            _machine = new Machine(new GCodeParser(new Utils.Logger()), App.TheApp.Server.Connection);
            
        }

        private void _timer_Tick(object sender, object e)
        {
            Current = DateTime.Now.ToString();
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs("Current"));
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await _machine.InitAsync();
            //_machine.StartWorkLoop();
            
        }

        private String _startup;
        public String Startup { get { return _startup; } }

        public String Current { get; set; }

        private void _appServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            if(args.Request.Message.ContainsKey("STATUS"))
            {
                var msg = args.Request.Message["STATUS"];
                Debug.WriteLine("STATUS: " + msg);
            }

            if (args.Request.Message.ContainsKey("DONE"))
            {
                var msg = args.Request.Message["DONE"];
                Debug.WriteLine("Axis Done: " + msg);
            }
        }

        private async void Forard_Click(object sender, RoutedEventArgs e)
        {
            /*var msg = new ValueSet();
            msg.Add("AXIS", Convert.ToInt32(4));
            msg.Add("MULTIPLIER", Convert.ToInt32(1));
            msg.Add("STEPS", Convert.ToInt32(20 * 300));
             await _appServiceConnection.SendMessageAsync(msg);*/
        }

        private async void Backwards_Click(object sender, RoutedEventArgs e)
        {
/*            var msg = new ValueSet();
            msg.Add("AXIS", Convert.ToInt32(3));
            msg.Add("MULTIPLIER", Convert.ToInt32(1));
            msg.Add("STEPS", Convert.ToInt32(20 * 300));
            await _appServiceConnection.SendMessageAsync(msg);*/
        }
    }
}
