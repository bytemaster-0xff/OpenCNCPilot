using System;
using System.Threading;
using System.Threading.Tasks;

namespace LagoViata.PNP.Channel
{
    public class Server : IServer
    {
        Windows.Networking.Sockets.StreamSocketListener _listener;

        bool _running = true;
        int _listenPort;

        Connection _connection;

        Timer _watchDog;

        public Server(int listenPort)
        {
            _listenPort = listenPort;
            _connection = new Connection();

            _watchDog = new Timer(WatchDogCallback, null, 0, 2500);

        }

        private void WatchDogCallback(object state)
        {
            if(_connection.IsOpen && _connection.Las)
        }
       
        private void _listener_ConnectionReceived(Windows.Networking.Sockets.StreamSocketListener sender, Windows.Networking.Sockets.StreamSocketListenerConnectionReceivedEventArgs args)
        {
            _connection.Open(args.Socket);
        }

        public async Task StartListeningAsync()
        {
            _listener = new Windows.Networking.Sockets.StreamSocketListener();
            _listener.ConnectionReceived += _listener_ConnectionReceived;
            await _listener.BindServiceNameAsync(_listenPort.ToString());
        }

        public Connection Connection { get { return _connection; } }
    }
}
