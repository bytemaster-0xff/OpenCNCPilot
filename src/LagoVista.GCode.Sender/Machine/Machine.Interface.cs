using LagoVista.Core.GCode;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender
{
    public partial class Machine
    {
        ISerialPort _port;
        ISocketClient _socketClient;

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
                    _sentQueue.Clear();
                    _toSendPriority.Clear();
                }

                Mode = OperatingMode.Manual;

                if (Settings.MachineType == FirmwareTypes.Repeteir_PnP)
                {
                    Enqueue("M43 P25");
                    Enqueue("M43 P27");
                    Enqueue("M43 P29");
                    Enqueue("M43 P31");
                    Enqueue("M43 P32");
                    Enqueue("M43 P33");
                    Enqueue("G90");
                }

                _cancelSource = new CancellationTokenSource();
                _port = port;

                AddStatusMessage(StatusMessageTypes.Info, $"Opened Serial Port");

                await Task.Run(() =>
                {
                    Work(port.InputStream, port.OutputStream);
                }, _cancelSource.Token);
            }
            catch (Exception ex)
            {
                _port = null;
                Connected = false;
                AddStatusMessage(StatusMessageTypes.Warning, $"Could not open serial port: " + ex.Message);
            }
        }

        CancellationTokenSource _cancelSource;

        public async Task ConnectAsync(ISocketClient socketClient)
        {
            _socketClient = socketClient;
            _cancelSource = new CancellationTokenSource();

            Connected = true;
            Mode = OperatingMode.Manual;

            AddStatusMessage(StatusMessageTypes.Info, $"Opened Network Connection");

            await Task.Run(() =>
            {
                Work(socketClient.InputStream, socketClient.OutputStream);
            }, _cancelSource.Token);
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
                WorkspacePosition = new Vector3();

                Status = "Disconnected";
                DistanceMode = ParseDistanceMode.Absolute;
                Unit = ParseUnit.Metric;
                Plane = ArcPlane.XY;
                UnacknowledgedBytesSent = 0;

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
                AddStatusMessage(StatusMessageTypes.Info, "Closed Serial Port");
                _port = null;
            }

            if (_socketClient != null)
            {
                await _socketClient.CloseAsync();
                _socketClient.Dispose();
                _socketClient = null;
            }

            if (this._cancelSource != null)
            {
                _cancelSource.Cancel();
                _cancelSource = null;
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

        public int ToSendQueueCount
        {
            get
            {
                lock (_queueAccessLocker)
                {
                    return _toSend.Count;
                }
            }
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
                    PendingQueue.Clear();
                    if (Settings.MachineType == FirmwareTypes.GRBL1_1)
                        _toSendPriority.Enqueue(((char)0x18).ToString());
                    else if (Settings.MachineType == FirmwareTypes.Repeteir_PnP)
                        _toSendPriority.Enqueue("M112");

                    Vacuum1On = false;
                    Vacuum2On = false;
                    SolendoidOn = false;
                }

                UnacknowledgedBytesSent = 0;
            }
        }

        public void Enqueue(String cmd, bool highPriority = false)
        {
            if (AssertConnected())
            {
                Services.DispatcherServices.Invoke(() =>
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
                            if (Settings.MachineType == FirmwareTypes.LagoVista_PnP ||
                                Settings.MachineType == FirmwareTypes.SimulatedMachine ||
                                Settings.MachineType == FirmwareTypes.Repeteir_PnP)
                                PendingQueue.Add(cmd);
                        }
                    }
                });
            }
        }

        //TODO: Thinking we need a better "emergency stop"
        public void EmergencyStop()
        {
            SoftReset();
        }

        public async void GotoWorkspaceHome()
        {
            Enqueue($"G0 Z{Settings.ToolSafeMoveHeight} F{Settings.FastFeedRate}");

            if (Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Enqueue("M57");
            }
            else if (Settings.MachineType == FirmwareTypes.Repeteir_PnP)
            {
                await SetViewTypeAsync(ViewTypes.Camera);
                GotoPoint(0, 0);
            }
            else
            {
                if (Settings.MachineType == FirmwareTypes.GRBL1_1)
                {
                    Enqueue("G0 Z20");
                }

                Enqueue("G0 X0 Y0");

                if (Settings.MachineType == FirmwareTypes.GRBL1_1)
                {
                    Enqueue("G0 Z0");
                }
            }
        }

        public void GotoFiducialHome()
        {
            Enqueue("M53");
        }

        public void SetWorkspaceHome()
        {
            if (Settings.MachineType == FirmwareTypes.Repeteir_PnP)
            {
                Enqueue("G92 X0 Y0 Z0");
            }
            else
            {
                Enqueue("M77");
            }
        }

        public void SetFavorite1()
        {
            Enqueue("M78");
        }

        public void SetFavorite2()
        {

            Enqueue("M79");
        }

        public void GotoFavorite1()
        {
            Enqueue("M58");
        }

        public void GotoFavorite2()
        {
            Enqueue("M59");
        }

        public void HomingCycle()
        {
            _viewType = ViewTypes.Camera;
            RaisePropertyChanged(nameof(ViewType));

            if (Settings.MachineType == FirmwareTypes.GRBL1_1)
            {
                Enqueue("$H\n", true);
            }
            else
            {
                Vacuum1On = false;
                Vacuum2On = false;
                SolendoidOn = false;
                Enqueue("G28");
                if (Settings.MachineType == FirmwareTypes.Repeteir_PnP)
                {

                    Enqueue($"G0 X{Settings.DefaultWorkspaceHome.X} Y{Settings.DefaultWorkspaceHome.Y} F{Settings.FastFeedRate}");
                    GotoPoint(Settings.DefaultWorkspaceHome.X, Settings.DefaultWorkspaceHome.Y);
                    SetWorkspaceHome();
                }
            }
        }

        public void FeedHold()
        {
            Enqueue("!", true);
        }

        public void ClearAlarm()
        {
            if (Settings.MachineType == FirmwareTypes.GRBL1_1)
            {
                Enqueue("$X\n", true);
            }
            else
            {
                _toSendPriority.Enqueue(((char)0x06).ToString());
            }
        }

        public void CycleStart()
        {
            Enqueue("~", true);
        }

        public void SpindleOn()
        {
            Enqueue("M3");
        }

        public void SpindleOff()
        {
            Enqueue("M5");
        }

        public void LaserOff()
        {
            Enqueue("M5");
        }

        public void LaserOn()
        {
            Enqueue("M3");
        }
    }
}
