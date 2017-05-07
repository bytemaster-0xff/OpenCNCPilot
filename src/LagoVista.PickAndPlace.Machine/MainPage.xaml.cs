using LagoViata.PNP.Services;
using LagoVista.Core.GCode.Parser;
using System;
using System.Collections.Generic;
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
    public sealed partial class MainPage : Page
    {
        Machine _machine;

        AppServiceConnection _appServiceConnection;

        public MainPage()
        {
            this.InitializeComponent();

            this.Loaded += MainPage_Loaded;

            _machine = new Machine(new GCodeParser(new Utils.Logger()), App.TheApp.Server.Connection);
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await _machine.InitAsync();
            //_machine.StartWorkLoop();

            var listing = await AppServiceCatalog.FindAppServiceProvidersAsync("LagoVistaGCodeAppService");
            var packageName = listing[0].PackageFamilyName;


            _appServiceConnection = new AppServiceConnection();
            _appServiceConnection.AppServiceName = "LagoVistaGCodeAppService";
            _appServiceConnection.PackageFamilyName = packageName;

            Debug.WriteLine("Got Here => " + packageName);

            var status = await _appServiceConnection.OpenAsync();
            if (status == AppServiceConnectionStatus.Success)
            {
                _appServiceConnection.RequestReceived += _appServiceConnection_RequestReceived;
            }
        }

        private void _appServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var msg = args.Request.Message["STATUS"];
            Debug.WriteLine(msg);
        }

        private async void Forard_Click(object sender, RoutedEventArgs e)
        {
            var msg = new ValueSet();
            msg.Add("AXIS", Convert.ToInt32(3));
            msg.Add("MULTIPLIER", Convert.ToInt32(2));
            msg.Add("STEPS", Convert.ToInt64(300));
            var sendstatus = await _appServiceConnection.SendMessageAsync(msg);
        }

        private void Backwards_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
