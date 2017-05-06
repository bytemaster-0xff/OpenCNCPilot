using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace LagoViata.PNP.Channel
{
    public interface IConnection
    {
        Task WriteAsync(string message);
        event EventHandler<string> LineReceivedEvent;
        event EventHandler<char> ImmediateCommandReceivedEvent;
        bool IsOpen { get; }
        void Open(StreamSocket socket);
        void Close();
    }
}
