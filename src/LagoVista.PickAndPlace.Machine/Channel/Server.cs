using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LagoViata.PNP.Channel
{
    public class Server
    {
        Windows.Networking.Sockets.StreamSocketListener _listener;

        bool _running = true;
        private const int PORT = 9001;

        Connection _connection;

        public bool IsConnected
        {
            get { return _connection != null; }
        }

        private void _listener_ConnectionReceived(Windows.Networking.Sockets.StreamSocketListener sender, Windows.Networking.Sockets.StreamSocketListenerConnectionReceivedEventArgs args)
        {
            _connection = Connection.Create(args.Socket);
            _connection.StartListening();
        }

        public async Task StartListeningAsync()
        {
            _listener = new Windows.Networking.Sockets.StreamSocketListener();
            _listener.ConnectionReceived += _listener_ConnectionReceived;

            await _listener.BindServiceNameAsync(PORT.ToString());

        }
    }
}
