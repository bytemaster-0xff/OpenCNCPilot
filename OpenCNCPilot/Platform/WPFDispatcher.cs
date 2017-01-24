using OpenCNCPilot.Core.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCNCPilot.Platform
{
    class WPFDispatcher : IDispatcher
    {
        public void RunOnUIThread(Action action)
        {
            App.Current.Dispatcher.BeginInvoke(action);
        }

        public void RunOnUIThread(Action<string> action, string param)
        {
            App.Current.Dispatcher.BeginInvoke(action, param);
        }
    }
}
