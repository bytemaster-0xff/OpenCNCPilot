using LagoVista.Core.WPF.PlatformSupport;
using OpenCNCPilot.Core;
using OpenCNCPilot.Core.Communication;
using System.Threading.Tasks;
using System.Windows;

namespace OpenCNCPilot
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
        static App _app;

        IMachine _machine;
        Settings _settings;

        public App()
        {
            _app = this;

            WPFDeviceServices.Init(Dispatcher);
        }

        public async Task InitAsync()
        {
            _settings = await Settings.LoadAsync();

            _machine = new Machine(_settings);
        }


        public OpenCNCPilot.Core.Settings Settings { get { return _settings; } }

        public new static App Current { get { return _app; } }

        public IMachine Machine { get { return _machine; } }

    }
}
