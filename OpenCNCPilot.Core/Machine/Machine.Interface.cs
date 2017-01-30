using LagoVista.Core.GCode;
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
                var outputStream = await port.OpenAsync();
                if (outputStream == null)
                {
                    AddStatusMessage(StatusMessageTypes.Warning, $"Could not open serial port.");
                    return;
                }

                Connected = true;

                lock (_queueAccessLocker)
                {
                    _toSend.Clear();
                    _sentQueue.Clear();
                    _toSendPriority.Clear();
                }

                Mode = OperatingMode.Manual;

                _cancelToken = new CancellationToken();
                _port = port;

                await Task.Run(() =>
                {
                    Work(outputStream);
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
                BufferState = 0;

                lock (_queueAccessLocker)
                {
                    _toSend.Clear();
                    _toSendPriority.Clear();
                    _sentQueue.Clear();
                }
            }

            if (_port != null)
            {
                await _port.CloseAsync();
            }
        }

        public void SendLine(string line)
        {
            if (AssertConnected())
            {
                if (Mode != OperatingMode.Manual && Mode != OperatingMode.ProbingHeightMap)
                {
                    AddStatusMessage(StatusMessageTypes.Warning, "Not In Manual Mode");
                    return;
                }

                lock (_toSend)
                {
                    _toSend.Enqueue(line);
                }
            }
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
            if (Mode != OperatingMode.Manual)
            {
                AddStatusMessage(StatusMessageTypes.Warning, "Busy");
                return false;
            }

            return true;
        }


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
                    _toSendPriority.Enqueue(((char)0x18).ToString());
                }

                BufferState = 0;
            }
        }


        private void Enqueue(String cmd)
        {
            if (AssertConnected())
            {
                lock (_queueAccessLocker)
                {
                    _toSendPriority.Enqueue(cmd);
                }
            }
        }

        public void FeedHold()
        {
            Enqueue("!");
        }

        public void ClearAlarm()
        {
            Enqueue("$X");
        }

        public void CycleStart()
        {
            Enqueue("~");
        }

        public void ProbeStart()
        {
            if (AssertConnected())
            {
                if (Mode != OperatingMode.Manual)
                {
                    AddStatusMessage(StatusMessageTypes.Warning, "Busy");
                    return;
                }

                Mode = OperatingMode.ProbingHeightMap;
            }
        }

        public void ProbeStop()
        {
            if (AssertConnected())
            {
                if (Mode != OperatingMode.ProbingHeightMap)
                {
                    AddStatusMessage(StatusMessageTypes.Warning, "Not in Probe Mode");
                    return;
                }

                Mode = OperatingMode.Manual;
            }
        }
    }
}
