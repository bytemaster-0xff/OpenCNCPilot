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

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            _app = this;

        }

        public OpenCNCPilot.Core.Settings Settings { get; private set; }

        public new static App Current { get { return _app; } }

        public IDispatcher DispatcherService { get { return _dispatcher; } }
        public IStorage StorageService { get { return _storage; } }

        public ILogger LoggerService { get { return _logger; } }

    }
}
