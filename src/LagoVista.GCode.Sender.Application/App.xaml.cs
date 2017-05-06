using LagoVista.Core.WPF.PlatformSupport;
using LagoVista.GCode.Sender.Application.Services;
using LagoVista.GCode.Sender.Interfaces;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : System.Windows.Application
	{
        static App _app;

        public App()
        {
            _app = this;

            LagoVista.Core.IOC.SLWIOC.Register<ISocketClient, SocketClient>();

            WPFDeviceServices.Init(Dispatcher);
        }

        public new static App Current { get { return _app; } }
    }
}
