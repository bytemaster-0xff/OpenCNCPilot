using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCodeSupport.Tests.Mocks
{
    public class FakePlatformSupport : ILogger
    {
        public static void Register()
        {
            SLWIOC.Register<ILogger>(new FakePlatformSupport());
        }

        public void Log(LogLevel level, string area, string message, params KeyValuePair<string, string>[] args)
        {
            Console.WriteLine($"{level} - {area} {message}");
        }

        public void LogException(string area, Exception ex, params KeyValuePair<string, string>[] args)
        {
            Console.WriteLine($"{area} - {ex.Message}");
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
            throw new NotImplementedException();
        }
    }
}
