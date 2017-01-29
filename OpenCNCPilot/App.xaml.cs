using LagoVista.Core.WPF.PlatformSupport;

using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : System.Windows.Application
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


        public Settings Settings { get { return _settings; } }

        public new static App Current { get { return _app; } }

        public IMachine Machine { get { return _machine; } }

    }
}
