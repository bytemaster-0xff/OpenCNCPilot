﻿using System;
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
                _writer.Write(_toSendPriority.Dequeue());
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

            BufferState += 1;

            _sentQueue.Enqueue(send_line);
            _toSend.Dequeue();
        }

        private async Task QueryStatus()
        {
            var Now = DateTime.Now;

            if ((Now - _lastPollTime).TotalMilliseconds > (Mode == OperatingMode.Manual ? _settings.StatusPollIntervalIdle : _settings.StatusPollIntervalRunning))
            {
                var statusRequest = _settings.MachineType == FirmwareTypes.GRBL1_1 ? "?" : "M114\n";
                _writer.Write(statusRequest);
                _writer.Flush();
                _lastPollTime = Now;
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
                //SendFile(_writer);
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

                BufferState = 0;

                if (_settings.MachineType == FirmwareTypes.GRBL1_1)
                {
                    _writer.Write("\n$G\n");
                    _writer.Flush();
                }

                while (true)
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