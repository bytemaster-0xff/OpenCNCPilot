using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoViata.PNP.Utils
{
    public class Logger : ILogger
    {
        public void Log(LogLevel level, string area, string message, params KeyValuePair<string, string>[] args)
        {
            Debug.Write($"{area} {message}");
        }

        public void LogException(string area, Exception ex, params KeyValuePair<string, string>[] args)
        {
            Debug.Write($"{area} {ex.Message}");
        }

        public void SetKeys(params string[] args)
        {
            throw new NotImplementedException();
        }

        public void SetUserId(string userId)
        {
            throw new NotImplementedException();
        }

        public void TrackEvent(string message, Dictionary<string, string> parameters)
        {

        }
    }
}
