using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCNCPilot.Core.Platform
{
    public interface IDispatcher
    {
        void RunOnUIThread(Action action);
        void RunOnUIThread(Action<string> action, string param);
    }
}
