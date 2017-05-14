using LagoVista.Core.Models;
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
    public class Connection : ModelBase, IConnection, IDisposable
    {
        const int MAX_BUFFER_SIZE = 65535;
        TimeSpan CONNECTION_TIMEOUT = TimeSpan.FromSeconds(5);

        StreamSocket _socket;
        StreamReader _reader;
        StreamWriter _writer;
        Stream _inputStream;
        Stream _outputStream;
        Task _listenerTask;
        CancellationTokenSource _cancelListenerSource;
        char[] _readBuffer;
        char[] _holdingBuffer;
        int _holdingBufferIndex = 0;
        DateTime? _lastMessageDateStamp;

        public event EventHandler<string> LineReceivedEvent;

        public event EventHandler<char> ImmediateCommandReceivedEvent;


        private List<char> _immediateChars = new List<char>()
        {
            '!',
            '?',
            (char)0x06,
            (char)0x18
        };

        public Connection()
        {
            _readBuffer = new char[MAX_BUFFER_SIZE];
            _holdingBuffer = new char[MAX_BUFFER_SIZE];
        }

        public DateTime LastMessageReceivedDateStamp
        {
            get { return _lastMessageDateStamp.HasValue ? _lastMessageDateStamp.Value : DateTime.MinValue ; }
        }

        private bool _isOpen;
        public bool IsOpen
        {
            get { return _isOpen; }
            set { Set(ref _isOpen, value); }
        }

        public void Open(StreamSocket socket)
        {
            _socket = socket;

            _inputStream = socket.InputStream.AsStreamForRead();
            _reader = new StreamReader(_inputStream);

            _outputStream = socket.OutputStream.AsStreamForWrite();
            _writer = new StreamWriter(_outputStream);

            StartListening();
        }


        public void Close()
        {
            Disconnect();
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
                        for(var idx = 0; idx < bytesRead; ++idx)
                        {
                            var ch = _readBuffer[idx];
                            if(_immediateChars.Contains(ch))
                            {
                                ImmediateCommandReceivedEvent?.Invoke(this, ch);
                                _holdingBufferIndex = 0;
                                _lastMessageDateStamp = DateTime.Now;
                            }
                            else if (ch == '\n')
                            {
                                _lastMessageDateStamp = DateTime.Now;
                                var cmd = new String(_holdingBuffer, 0, _holdingBufferIndex);
                                LineReceivedEvent?.Invoke(this, cmd);
                                _holdingBufferIndex = 0;
                            }
                            else
                            {
                                _holdingBuffer[_holdingBufferIndex++] = _readBuffer[idx];
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        running = false;
                        Cleanup();
                        _isOpen = false;
                    }
                    catch (Exception)
                    {
                        running = false;
                    }
                }

            });

            _listenerTask.Start();
        }
        

        public void Disconnect()
        {
            _cancelListenerSource.Cancel();
        }


        public async Task WriteAsync(byte[] buffer)
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
    
        public Task WriteAsync(string message)
        {
            return WriteAsync(System.Text.ASCIIEncoding.ASCII.GetBytes(message));
        }

        private void Cleanup()
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
       
        public void Dispose()
        {
            lock (this)
            {
                Cleanup();
            }
        }
    }
}
