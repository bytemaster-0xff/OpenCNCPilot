using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace LagoViata.PNP.Channel
{
    public class Connection : IDisposable
    {
        const int MAX_BUFFER_SIZE = 1024;
        TimeSpan CONNECTION_TIMEOUT = TimeSpan.FromSeconds(5);

        StreamSocket _socket;
        StreamReader _reader;
        StreamWriter _writer;
        Stream _inputStream;
        Stream _outputStream;
        Task _listenerTask;
        CancellationTokenSource _cancelListenerSource;
        char[] _readBuffer;
        DateTime? _lastMessageDateStamp;

        public Connection(StreamSocket socket)
        {
            _socket = socket;
            _socket = socket;

            _inputStream = socket.InputStream.AsStreamForRead();
            _reader = new StreamReader(_inputStream);

            _outputStream = socket.OutputStream.AsStreamForWrite();
            _writer = new StreamWriter(_outputStream);

            _lastMessageDateStamp = DateTime.Now;

            _readBuffer = new char[MAX_BUFFER_SIZE];

        }

        public void StartListening()
        {
            _cancelListenerSource = new CancellationTokenSource();
            _listenerTask = new Task(async () =>
            {
                var running = true;
                while (running)
                {
                    try
                    {
                        var readTask = _reader.ReadAsync(_readBuffer, 0, MAX_BUFFER_SIZE);
                        readTask.Wait(_cancelListenerSource.Token);
                        var bytesRead = await readTask;

                        var byteBuffer = _readBuffer.ToByteArray(0, bytesRead);
                        var msg = System.Text.ASCIIEncoding.ASCII.GetString(byteBuffer);
                        Debug.WriteLine(msg);
                        var buffer = System.Text.ASCIIEncoding.ASCII.GetBytes("HELLO WORLD");
                        await Write(buffer);
                    }
                    catch (OperationCanceledException)
                    {
                        running = false;
                        /* Task Cancellation */
                    }
                    catch (Exception ex)
                    {
                        running = false;
                    }
                }

            });

            _listenerTask.Start();
        }

        public bool IsConnected
        {
            get { return (DateTime.Now - _lastMessageDateStamp) < CONNECTION_TIMEOUT; }
        }

        public void Disconnect()
        {
            _cancelListenerSource.Cancel();
        }


        public async Task Write(byte[] buffer)
        {
            if (_writer != null)
            {
                try
                {
                    await _writer.WriteAsync(buffer.ToCharArray());
                    await _writer.FlushAsync();
                }
                catch (Exception)
                {
                    Disconnect();
                }
            }
        }

        public static Connection Create(StreamSocket socket)
        {
            return new Connection(socket);
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_reader != null)
                {
                    _reader.Dispose();
                    _reader = null;
                }

                if (_writer != null)
                {
                    _writer.Dispose();
                    _writer = null;
                }

                if (_inputStream != null)
                {
                    _inputStream.Dispose();
                    _inputStream = null;
                }

                if (_outputStream != null)
                {
                    _outputStream.Dispose();
                    _outputStream = null;
                }

                if (_socket != null)
                {
                    _socket.Dispose();
                    _socket = null;
                }
            }
        }
    }
}
