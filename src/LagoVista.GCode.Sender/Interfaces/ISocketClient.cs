using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Interfaces
{
    public interface ISocketClient
    {
        Task ConnectAsync(String ipAddress, int port);
        Stream InputStream { get; }
        Stream OutputStream { get; }
    }
}
