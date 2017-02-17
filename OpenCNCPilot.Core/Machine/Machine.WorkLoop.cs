﻿using LagoVista.Core.GCode.Commands;
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
                if (line != "?")
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

            UpdateStatus(send_line.ToString());
            AddStatusMessage(StatusMessageTypes.SentLine, send_line.ToString());

            UnacknowledgedBytesSent += send_line.Length;
            UnacknowledgedBytesSent += 1;

            _sentQueue.Enqueue(send_line);
            _toSend.Dequeue();
        }

        private async void SendJobItems()
        {
            var sendCommand = _jobToSend.Peek();

            if (sendCommand.Command == "M06")
            {
                Mode = OperatingMode.PendingToolChange;
                var machineCommand = sendCommand as MCode;
                await Services.Popups.ShowAsync("Tool Change " + machineCommand.DrillSize.ToString());
                Mode = OperatingMode.SendingJob;
            }

            _writer.Write(sendCommand.Line);
            _writer.Write('\n');
            _writer.Flush();

            UpdateStatus(sendCommand.Line.ToString());
            AddStatusMessage(StatusMessageTypes.SentLine, sendCommand.Line.ToString());

            UnacknowledgedBytesSent += sendCommand.Line.Length;
            UnacknowledgedBytesSent += 1;

            _sentQueue.Enqueue(sendCommand.Line);
            _jobToSend.Dequeue();
        }

        private async Task QueryStatus()
        {
            var Now = DateTime.Now;

            if (Mode == OperatingMode.Manual)
            {
                if ((Now - _lastPollTime).TotalMilliseconds > _settings.StatusPollIntervalIdle)
                {
                    if (_settings.MachineType == FirmwareTypes.GRBL1_1)
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
            else if (Mode == OperatingMode.SendingJob)
            {
                if ((Now - _lastPollTime).TotalMilliseconds > _settings.StatusPollIntervalRunning)
                {
                    MachinePosition = JobManager.CurrentCommand.CurrentPosition;
                    WorkPosition = JobManager.CurrentCommand.CurrentPosition;
                    _lastPollTime = Now;
                }
            }

            await Task.Delay(_waitTime);
        }

        private bool ShouldSendNormalPriorityItems()
        {
            return _toSend.Count > 0 && ((_toSend.Peek().ToString()).Length + 1) < (_settings.ControllerBufferSize - UnacknowledgedBytesSent);
        }

        private bool ShouldSendJobItems()
        {
            return _jobToSend.Count > 0 && ((_jobToSend.Peek().ToString()).Length + 1) < (_settings.ControllerBufferSize - UnacknowledgedBytesSent);
        }

        private async Task Send()
        {
            SendHighPriorityItems();

            if (Mode == OperatingMode.SendingJob)
            {
                JobManager.ProcessNextLines();
            }

            if(ShouldSendJobItems() && Mode == OperatingMode.SendingJob)
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

        private async void Work(Stream stream)
        {
            try
            {
                _waitTime = TimeSpan.FromMilliseconds(0.5);
                _lastPollTime = DateTime.Now + TimeSpan.FromSeconds(0.5);
                _connectTime = DateTime.Now;

                _reader = new StreamReader(stream);
                _writer = new StreamWriter(stream);

                UnacknowledgedBytesSent = 0;

                if (_settings.MachineType == FirmwareTypes.GRBL1_1)
                {
                    Enqueue("\n$G\n", true);
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
