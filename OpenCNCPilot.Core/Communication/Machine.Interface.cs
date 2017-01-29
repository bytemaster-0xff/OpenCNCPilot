using LagoVista.Core.PlatformSupport;
using OpenCNCPilot.Core.GCode;
using OpenCNCPilot.Core.GCode.GCodeCommands;
using OpenCNCPilot.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCNCPilot.Core.Communication
{
    public partial class Machine
    {
        ISerialPort _port;

        public async Task ConnectAsync(ISerialPort port)
        {
            try
            {
                if (Connected)
                    throw new Exception("Can't Connect: Already Connected");

                var outputStream = await port.OpenAsync();
                if (outputStream == null)
                {
                    RaiseEvent(ReportError, $"Could not open serial port.");
                    return;
                }

                Connected = true;

                _toSend.Clear();
                _sentQueue.Clear();

                Mode = OperatingMode.Manual;

                if (PositionUpdateReceived != null)
                    PositionUpdateReceived.Invoke();

                _cancelToken = new CancellationToken();
                _port = port;

                await Task.Run(() =>
                {
                    Work(outputStream);
                }, _cancelToken);
            }
            catch(Exception ex)
            {
                _port = null;
                Connected = false;
                RaiseEvent(ReportError, $"Could not open serial port: " + ex.Message);
            }
        }

        public async Task DisconnectAsync()
        {
            if (!Connected)
                throw new Exception("Can't Disconnect: Not Connected");

            Mode = OperatingMode.Disconnected;
            Connected = false;

            await _port.CloseAsync();

            MachinePosition = new Vector3();
            WorkPosition = new Vector3();

            if (PositionUpdateReceived != null)
                PositionUpdateReceived.Invoke();

            Status = "Disconnected";
            DistanceMode = ParseDistanceMode.Absolute;
            Unit = ParseUnit.Metric;
            Plane = ArcPlane.XY;
            BufferState = 0;

            _toSend.Clear();
            _toSendPriority.Clear();
            _sentQueue.Clear();
        }

        public void SendLine(string line)
        {
            if (!Connected)
            {
                RaiseEvent(Info, "Not Connected");
                return;
            }

            if (Mode != OperatingMode.Manual && Mode != OperatingMode.ProbingHeightMap)
            {
                RaiseEvent(Info, "Not in Manual Mode");
                return;
            }

            lock (_toSend)
            {
                _toSend.Enqueue(line);
            }
        }

        public void SoftReset()
        {
            if (!Connected)
            {
                RaiseEvent(Info, "Not Connected");
                return;
            }

            Mode = OperatingMode.Manual;

            lock (_toSend)
                lock (_toSendPriority)
                    lock (_sentQueue)
                    {
                        _toSend.Clear();
                        _toSendPriority.Clear();
                        _sentQueue.Clear();
                        _toSendPriority.Enqueue(((char)0x18).ToString());
                    }

            BufferState = 0;
        }

        public void FeedHold()
        {
            if (!Connected)
            {
                RaiseEvent(Info, "Not Connected");
                return;
            }

            lock (_toSendPriority)
            {
                _toSendPriority.Enqueue("!");
            }
        }

        public void CycleStart()
        {
            if (!Connected)
            {
                RaiseEvent(Info, "Not Connected");
                return;
            }

            lock (_toSendPriority)
            {
                _toSendPriority.Enqueue("~");
            }
        }

        public void ProbeStart()
        {
            if (!Connected)
            {
                RaiseEvent(Info, "Not Connected");
                return;
            }

            if (Mode != OperatingMode.Manual)
            {
                RaiseEvent(Info, "Can't start probing while running!");
                return;
            }

            Mode = OperatingMode.ProbingHeightMap;
        }

        public void ProbeStop()
        {
            if (!Connected)
            {
                RaiseEvent(Info, "Not Connected");
                return;
            }

            if (Mode != OperatingMode.ProbingHeightMap)
            {
                RaiseEvent(Info, "Not in Probe mode");
                return;
            }

            Mode = OperatingMode.Manual;
        }
    }
}
