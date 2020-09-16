using LagoVista.Core.WPF.PlatformSupport;
using LagoVista.GCode.Sender.Interfaces;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.Services
{
    public class SocketClient : ISocketClient
    {
        TcpClient _client;

        public async Task ConnectAsync(string ipAddress, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ipAddress, 9001);
            InputStream = _client.GetStream();
            OutputStream = _client.GetStream();
        }

        public Stream InputStream { get; private set; }
        public Stream OutputStream { get; private set; }
    }
}
