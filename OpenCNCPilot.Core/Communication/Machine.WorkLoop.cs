using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCNCPilot.Core.Communication
{
    public partial class Machine
    {
        DateTime _connectTime;

        Queue<string> _sentQueue = new Queue<string>();
        Queue<string> _toSend = new Queue<string>();
        Queue<string> _toSendPriority = new Queue<string>();

        private String GetNextLine()
        {
            bool eof = false;
            String send_line = null;
            while (send_line == null && !eof)
            {
                if (File.Count > FilePosition && 
                    (File[FilePosition].Length + 1) < (_settings.ControllerBufferSize - BufferState))
                {
                    var nextLine = File[FilePosition++];
                    var parts = nextLine.Split(';');
                    if (!String.IsNullOrEmpty(parts[0]))
                    {
                        send_line = parts[0];
                    }
                }
                else
                {
                    eof = true;
                }
            }

            return send_line;
        }

        private void SendFile(StreamWriter writer)
        {
            var send_line = GetNextLine();
            if (!String.IsNullOrEmpty(send_line))
            {
                writer.Write(send_line);
                writer.Write('\n');
                writer.Flush();

                RaiseEvent(UpdateStatus, send_line);
                RaiseEvent(LineSent, send_line);

                BufferState += send_line.Length + 1;

                _sentQueue.Enqueue(send_line);

                if (FilePosition >= File.Count)
                {
                    Mode = OperatingMode.Manual;
                }
            }
        }

        private void ProcessResponseLine(String line)
        {
            if (line == "ok")
            {
                if (_sentQueue.Count != 0)
                {
                    BufferState -= ((string)_sentQueue.Dequeue()).Length + 1;
                }
                else
                {
                    LagoVista.Core.PlatformSupport.Services.Logger.Log(LagoVista.Core.PlatformSupport.LogLevel.Warning, "Machine_Work", "Received OK without anything in the Sent Buffer");
                    BufferState = 0;
                }
            }
            else if (line != null)
            {
                if (line.StartsWith("error: "))
                {
                    if (_sentQueue.Count != 0)
                    {
                        var errorline = _sentQueue.Dequeue();

                        RaiseEvent(ReportError, $"{line}: {errorline}");
                        BufferState -= errorline.Length + 1;
                    }
                    else
                    {
                        if ((DateTime.Now - _connectTime).TotalMilliseconds > 200)
                            RaiseEvent(ReportError, $"Received <{line}> without anything in the Sent Buffer");

                        BufferState = 0;
                    }

                    Mode = OperatingMode.Manual;
                }
                else if (line.StartsWith("<"))
                    RaiseEvent(ParseStatus, line);
                else if (line.StartsWith("[PRB:"))
                {
                    RaiseEvent(ParseProbe, line);
                    RaiseEvent(LineReceived, line);
                }
                else if (line.StartsWith("["))
                {
                    RaiseEvent(UpdateStatus, line);
                    RaiseEvent(LineReceived, line);
                }
                else if (line.StartsWith("ALARM"))
                {
                    RaiseEvent(NonFatalException, line);
                    Mode = OperatingMode.Manual;
                }
                else if (line.Length > 0)
                    RaiseEvent(LineReceived, line);
            }
            else
            {
                RaiseEvent(ReportError, $"Empty Response From Machine.");
            }
        }

        private async void Work(Stream stream)
        {
            try
            {
                StreamReader reader = new StreamReader(stream);
                StreamWriter writer = new StreamWriter(stream);
                
                BufferState = 0;

                TimeSpan WaitTime = TimeSpan.FromMilliseconds(0.5);
                DateTime LastStatusPoll = DateTime.Now + TimeSpan.FromSeconds(0.5);
                _connectTime = DateTime.Now;

                writer.Write("\n$G\n");
                writer.Flush();

                while (true)
                {
                    Task<string> lineTask = reader.ReadLineAsync();

                    while (!lineTask.IsCompleted)
                    {
                        if (!Connected)
                        {
                            return;
                        }

                        while (_toSendPriority.Count > 0)
                        {
                            writer.Write(_toSendPriority.Dequeue());
                            writer.Flush();
                        }
                        if (Mode == OperatingMode.SendFile)
                        {
                            SendFile(writer);
                            DateTime Now = DateTime.Now;

                            if ((Now - LastStatusPoll).TotalMilliseconds > _settings.StatusPollIntervalRunning)
                            {
                                writer.Write('?');
                                writer.Flush();
                                LastStatusPoll = Now;
                            }

                            await Task.Delay(WaitTime);

                        }
                        else if (_toSend.Count > 0 && ((_toSend.Peek().ToString()).Length + 1) < (_settings.ControllerBufferSize - BufferState))
                        {
                            var send_line = _toSend.Peek();

                            writer.Write(send_line);
                            writer.Write('\n');
                            writer.Flush();

                            RaiseEvent(UpdateStatus, send_line.ToString());
                            RaiseEvent(LineSent, send_line.ToString());

                            BufferState += 1;

                            _sentQueue.Enqueue(send_line);
                            _toSend.Dequeue();

                            continue;
                        }
                        else
                        {
                            DateTime Now = DateTime.Now;

                            if ((Now - LastStatusPoll).TotalMilliseconds > (Mode == OperatingMode.Manual ? _settings.StatusPollIntervalIdle : _settings.StatusPollIntervalRunning))
                            {
                                writer.Write('?');
                                writer.Flush();
                                LastStatusPoll = Now;
                            }

                            await Task.Delay(WaitTime);
                        }
                    }

                    ProcessResponseLine(lineTask.Result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                RaiseEvent(ReportError, $"Fatal Error: {ex.Message}");
                Disconnect();
            }
        }
    }
}
