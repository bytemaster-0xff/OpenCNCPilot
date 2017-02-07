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

            if(send_line.StartsWith("M06"))
            {
                Services.Popups.ShowAsync("Tool Change ");
            }

            _writer.Write(send_line);
            _writer.Write('\n');
            _writer.Flush();

            UpdateStatus(send_line.ToString());
            AddStatusMessage(StatusMessageTypes.SentLine, send_line.ToString());

            BufferState += send_line.Length;
            BufferState += 1;

            _sentQueue.Enqueue(send_line);
            _toSend.Dequeue();
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
                    MachinePosition = _jobProcessor.CurrentCommand.CurrentPosition;
                    WorkPosition = _jobProcessor.CurrentCommand.CurrentPosition;
                    _lastPollTime = Now;
                }
            }

            await Task.Delay(_waitTime);
        }

        private bool ShouldSendNormalPriorityItems()
        {
            return _toSend.Count > 0 && ((_toSend.Peek().ToString()).Length + 1) < (_settings.ControllerBufferSize - BufferState);
        }

        private async Task Send()
        {
            SendHighPriorityItems();

            if (Mode == OperatingMode.SendingJob && CurrentJob != null)
            {
                CurrentJob.Process();
            }

            if (ShouldSendNormalPriorityItems())
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

                BufferState = 0;

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
                RaiseEvent(ReportError, $"Fatal Error: {ex.Message}");
                await DisconnectAsync();
            }
        }
    }
}
