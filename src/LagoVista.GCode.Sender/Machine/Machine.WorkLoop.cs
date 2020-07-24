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
        Queue<GCodeCommand> _jobToSend = new Queue<GCodeCommand>();
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

            _writer.Write(send_line);
            _writer.Write('\n');
            _writer.Flush();

            if (send_line != "M114")
            {
                UpdateStatus(send_line.ToString());
                AddStatusMessage(StatusMessageTypes.SentLine, send_line.ToString());
            }

            _sentQueue.Enqueue(send_line);
            _toSend.Dequeue();
        }

        private void SendJobItems()
        {
            var sendCommand = _jobToSend.Peek();

            /* Make sure we normalize the line ending so it's only \n */
            _writer.Write(sendCommand.Line.Trim('\r', '\n'));
            _writer.Write('\n');
            _writer.Flush();

            Debug.WriteLine(">>> " + sendCommand.Line);

            UpdateStatus(sendCommand.Line.ToString());
            AddStatusMessage(StatusMessageTypes.SentLine, sendCommand.Line.ToString());

            _sentQueue.Enqueue(sendCommand.Line);
            _jobToSend.Dequeue();
        }

        private async Task QueryStatus()
        {
            var Now = DateTime.Now;

            if (Mode == OperatingMode.Manual)
            {
                if ((Now - _lastPollTime).TotalMilliseconds > Settings.StatusPollIntervalIdle)
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
                        if (Settings.MachineType == FirmwareTypes.GRBL1_1)
                        {
                            Enqueue("?", true);
                        }
                        else
                        {
                            //Enqueue("M114");
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
            return _toSend.Count > 0 && ((_toSend.Peek().ToString()).Length + 1) < (Settings.ControllerBufferSize - Math.Max(0,UnacknowledgedBytesSent));
        }

        private bool ShouldSendJobItems()
        {
            //return _jobToSend.Count > 0 && ((_jobToSend.Peek().ToString()).Length + 1) < (_settings.ControllerBufferSize - Math.Max(0,UnacknowledgedBytesSent));
            return _jobToSend.Count > 0;
        }

        private async Task Send()
        {
            SendHighPriorityItems();

            if (Mode == OperatingMode.SendingGCodeFile)
            {
                GCodeFileManager.ProcessNextLines();
            }

            if (ShouldSendJobItems() && Mode == OperatingMode.SendingGCodeFile)
            {
                SendJobItems();
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

            ProcessResponseLine(lineTask.Result);
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
