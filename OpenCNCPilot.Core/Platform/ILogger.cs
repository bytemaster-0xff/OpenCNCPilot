using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCNCPilot.Core.Platform
{
    public interface ILogger
    {
        void WriteLine(string message);
    }
}
