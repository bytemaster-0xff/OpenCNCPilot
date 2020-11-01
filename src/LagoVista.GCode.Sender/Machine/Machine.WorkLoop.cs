using LagoVista.Core.GCode.Commands;
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender
{
    public partial class Machine
    {
        DateTime _connectTime;

        Queue<string> _sentQueue = new Queue<string>();
        Queue<string> _toSend = new Queue<string>();
        Queue<string> _toSendPriority = new Queue<string>();

        StreamReader _reader;
        StreamWriter _writer;

        DateTime _lastPollTime;
        TimeSpan _waitTime;

        private void SendHighPriorityItems()
        {
            while (_toSendPriority.Count > 0)
            {
                var line = _toSendPriority.Dequeue();
                _writer.Write(line);
                if (line != "?" && line != "M114")
                {
                    AddStatusMessage(StatusMessageTypes.SentLinePriority, line.TrimStart().TrimEnd());
                }

                _writer.Flush();
            }
        }
        private void SendNormalPriorityItems()
        {
            var send_line = _toSend.Peek();
            UnacknowledgedBytesSent += send_line.Length + 1;

            if (send_line == "M400" || send_line == "G28")
            {
                _isOnHold = true;
            }

            _writer.Write(send_line);
            _writer.Write('\n');
            _writer.Flush();

            if (send_line != "M114")
            {
                Debug.WriteLine("SENDING LINE => " + send_line);

                UpdateStatus(send_line.ToString());
                AddStatusMessage(StatusMessageTypes.SentLine, send_line.ToString());
            }

            if (send_line == "M400")
            {
                _isOnHold = true;
            }

            _sentQueue.Enqueue(send_line);
            _toSend.Dequeue();
        }

        private void TransmitJobItem(GCodeCommand cmd)
        {
            var trimmedLine = cmd.Line.Trim('\r', '\n');

            if (trimmedLine == "M400" || trimmedLine == "G28")
            {
                _isOnHold = true;
            }

            UnacknowledgedBytesSent += trimmedLine.Length + 1;

            /* Make sure we normalize the line ending so it's only \n */
            _writer.Write(trimmedLine);
            _writer.Write('\n');
            _writer.Flush();

            Debug.WriteLine(">>> " + trimmedLine);

            cmd.Status = GCodeCommand.StatusTypes.Sent;

            UpdateStatus(cmd.Line.ToString());
            AddStatusMessage(StatusMessageTypes.SentLine, cmd.Line.ToString());

            _sentQueue.Enqueue(cmd.Line);
        }

        private async Task QueryStatus()
        {
            var Now = DateTime.Now;

            if (Mode == OperatingMode.Manual)
            {
                if ((Now - _lastPollTime).TotalMilliseconds > Settings.StatusPollIntervalIdle && LocationUpdateEnabled)
                {
                    if (Settings.MachineType == FirmwareTypes.GRBL1_1 ||
                        Settings.MachineType == FirmwareTypes.LagoVista ||
                        Settings.MachineType == FirmwareTypes.LagoVista_PnP)
                    {
                        Enqueue("?", true);
                    }
                    else
                    {
                        Enqueue("M114");
                    }

                    _lastPollTime = Now;
                }
            }
            else if (Mode == OperatingMode.SendingGCodeFile)
            {
                if ((Now - _lastPollTime).TotalMilliseconds > Settings.StatusPollIntervalRunning)
                {
                    if (Settings.CurrentSerialPort.Name == "Simulated")
                        MachinePosition = GCodeFileManager.CurrentCommand.CurrentPosition;
                    else
                    {
                        if (Settings.MachineType == FirmwareTypes.GRBL1_1 && LocationUpdateEnabled)
                        {
                            Enqueue("?", true);
                        }
                        else
                        {
                            Enqueue("M114");
                        }

                    }

                    _lastPollTime = Now;
                }
            }
            else if (Mode == OperatingMode.ProbingHeightMap)
            {
                if ((Now - _lastPollTime).TotalMilliseconds > Settings.StatusPollIntervalRunning)
                {
                    if (Settings.MachineType == FirmwareTypes.GRBL1_1)
                    {
                        Enqueue("?", true);
                    }
                    else
                    {
                        Enqueue("M114");
                    }

                    _lastPollTime = Now;
                }
            }

            await Task.Delay(_waitTime);
        }

        private bool ShouldSendNormalPriorityItems()
        {
            return _toSend.Count > 0 && ((_toSend.Peek().ToString()).Length + 1) < (Settings.ControllerBufferSize - Math.Max(0, UnacknowledgedBytesSent));
        }

        private async Task Send()
        {
            SendHighPriorityItems();

            if (!_isOnHold)
            {
                if (Mode == OperatingMode.SendingGCodeFile && 
                    _toSend.Count == 0 &&
                    Settings.ControllerBufferSize - Math.Max(0, UnacknowledgedBytesSent) > 24)
                {
                    var nextCommand = GCodeFileManager.GetNextJobItem();
                    if (nextCommand != null)
                        TransmitJobItem(nextCommand);
                }
                else if (ShouldSendNormalPriorityItems())
                {
                    SendNormalPriorityItems();
                }
                else
                {
                    await QueryStatus();
                }
            }
        }

        private async Task WorkLoop()
        {
            var lineTask = _reader.ReadLineAsync();

            /* While we are awaiting for a line to come in process any outgoing stuff */
            while (!lineTask.IsCompleted)
            {
                if (!Connected)
                {
                    return;
                }

                await Send();
            }

            if (lineTask.IsCompleted)
            {
                ProcessResponseLine(lineTask.Result);
            }
        }

        private async void Work(Stream inputStream, Stream outputStream)
        {
            try
            {
                _waitTime = TimeSpan.FromMilliseconds(0.5);
                _lastPollTime = DateTime.Now + TimeSpan.FromSeconds(0.5);
                _connectTime = DateTime.Now;

                _reader = new StreamReader(inputStream);
                _writer = new StreamWriter(outputStream);

                UnacknowledgedBytesSent = 0;

                if (Settings.MachineType == FirmwareTypes.GRBL1_1)
                {
                    Enqueue("\n$G\n", true);
                }
                else if (Settings.MachineType == FirmwareTypes.LagoVista_PnP)
                {
                    Enqueue("*", true);
                }

                while (Connected)
                {
                    await WorkLoop();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                AddStatusMessage(StatusMessageTypes.FatalError, $"Fatal Error: {ex.Message}");
                await DisconnectAsync();
            }
        }
    }
}
