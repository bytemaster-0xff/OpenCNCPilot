using OpenCNCPilot.Core.Platform;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
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

        IDispatcher _dispatcher;
        IStorage _storage;
        ILogger _logger;

        public App()
        {
            _app = this;
            _dispatcher = new Platform.WPFDispatcher();
            _storage = new Platform.WPFStorage();
            _logger = new Platform.WPFLogger();
        }


        public OpenCNCPilot.Core.Settings Settings { get; set; }

        public new static App Current { get { return _app; } }

        public IDispatcher DispatcherService { get { return _dispatcher; } }
        public IStorage StorageService { get { return _storage; } }

        public ILogger LoggerService { get { return _logger; } }

    }
}
