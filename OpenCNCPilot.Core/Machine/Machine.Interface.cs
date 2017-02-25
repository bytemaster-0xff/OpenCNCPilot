﻿using LagoVista.Core.GCode;
using LagoVista.Core.GCode.Commands;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender
{
    public partial class Machine
    {
        ISerialPort _port;

        public async Task ConnectAsync(ISerialPort port)
        {
            if (Connected)
                throw new Exception("Can't Connect: Already Connected");

            try
            {
                await port.OpenAsync();

                var outputStream = port.OutputStream;
                if (outputStream == null)
                {
                    AddStatusMessage(StatusMessageTypes.Warning, $"Could not open serial port");
                    return;
                }

                Connected = true;

                lock (_queueAccessLocker)
                {
                    _toSend.Clear();
                    _jobToSend.Clear();
                    _sentQueue.Clear();
                    _toSendPriority.Clear();
                }

                Mode = OperatingMode.Manual;

                _cancelToken = new CancellationToken();
                _port = port;

                AddStatusMessage(StatusMessageTypes.Info, $"Opened Serial Port");


                await Task.Run(() =>
                {
                    Work(port.InputStream, port.OutputStream);
                }, _cancelToken);
            }
            catch (Exception ex)
            {
                _port = null;
                Connected = false;
                AddStatusMessage(StatusMessageTypes.Warning, $"Could not open serial port: " + ex.Message);
            }
        }

        private object _queueAccessLocker = new object();

        public async Task DisconnectAsync()
        {
            lock (this)
            {
                if (!Connected)
                {
                    AddStatusMessage(StatusMessageTypes.Warning, "Can not disconnected - Not Connected");
                    return;
                }

                Mode = OperatingMode.Disconnected;
                Connected = false;

                MachinePosition = new Vector3();
                WorkPosition = new Vector3();

                Status = "Disconnected";
                DistanceMode = ParseDistanceMode.Absolute;
                Unit = ParseUnit.Metric;
                Plane = ArcPlane.XY;
                UnacknowledgedBytesSent = 0;

                lock (_queueAccessLocker)
                {
                    _jobToSend.Clear();

                    _toSend.Clear();
                    _toSendPriority.Clear();
                    _sentQueue.Clear();
                }
            }

            if (_port != null)
            {
                await _port.CloseAsync();
                AddStatusMessage(StatusMessageTypes.Info, "Closed Serial Port");
                _port = null;
            }

            AddStatusMessage(StatusMessageTypes.Info, "Disconnected");
        }

        private bool AssertConnected()
        {
            if (!Connected)
            {
                AddStatusMessage(StatusMessageTypes.Warning, "Not Connected");
                return false;
            }

            return true;
        }

        private bool AssertNotBusy()
        {
            if (Mode != OperatingMode.Manual && Mode != OperatingMode.Disconnected)
            {
                AddStatusMessage(StatusMessageTypes.Warning, "Busy");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Send CTRL-X to the machine
        /// </summary>
        public void SoftReset()
        {
            if (AssertConnected())
            {

                Mode = OperatingMode.Manual;

                lock (_queueAccessLocker)
                {
                    _toSend.Clear();
                    _toSendPriority.Clear();
                    _sentQueue.Clear();
                    _jobToSend.Clear();
                    _toSendPriority.Enqueue(((char)0x18).ToString());
                }

                UnacknowledgedBytesSent = 0;
            }
        }


        private void Enqueue(String cmd, bool highPriority = false)
        {
            if (AssertConnected())
            {
                lock (_queueAccessLocker)
                {
                    if (highPriority)
                    {
                        _toSendPriority.Enqueue(cmd);
                    }
                    else
                    {
                        _toSend.Enqueue(cmd);
                        UnacknowledgedBytesSent += cmd.Length + 1;
                    }
                }
            }
        }

        private void Enqueue(GCodeCommand cmd)
        {
            if (AssertConnected())
            {
                lock (_queueAccessLocker)
                {
                    _jobToSend.Enqueue(cmd);
                    UnacknowledgedBytesSent += cmd.Line.Length + 1;
                }
            }
        }

        public void EmergencyStop()
        {

        }

        public void SetFavorite1()
        {
            Enqueue("G28.1");
        }

        public void SetFavorite2()
        {

            Enqueue("G30.1");
        }

        public void GotoFavorite1()
        {
            Enqueue("G28");
        }

        public void GotoFavorite2()
        {
            Enqueue("G30");
        }

        public void HomingCycle()
        {
            Enqueue("$H\n", true);
        }

        public void FeedHold()
        {
            Enqueue("!", true);
        }

        public void ClearAlarm()
        {
            Enqueue("$X\n", true);
        }

        public void CycleStart()
        {
            Enqueue("~", true);
        }
       
    }
}
