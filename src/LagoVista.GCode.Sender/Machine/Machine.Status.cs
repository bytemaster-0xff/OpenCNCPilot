using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LagoVista.GCode.Sender
{
    public enum ViewTypes
    {
        Moving,
        Camera,
        Tool1,
        Tool2,
    }

    public partial class Machine
    {     
        public async Task SetViewTypeAsync(ViewTypes viewType)
        {
            if(ViewType == viewType)
            {
                return;
            }

            if (Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                _viewType = viewType;
                RaisePropertyChanged();

                switch (viewType)
                {
                    case ViewTypes.Camera: Enqueue("M50"); break;
                    case ViewTypes.Tool1: Enqueue("M51"); break;
                    case ViewTypes.Tool2: Enqueue("M51"); break;
                }
            }
            else if (Settings.MachineType == FirmwareTypes.Repeteir_PnP)
            {
                // 1. capture current position of machine.
                var currentLocation = MachinePosition;

                await Task.Run(() =>
                {
                    // 2. set relative move
                    Enqueue("G91"); // relative move


                    // 3. move the machine to the tool that should be used. 
                    if (_viewType == ViewTypes.Camera && viewType == ViewTypes.Tool1)
                    {
                        _viewType = ViewTypes.Moving;
                        RaisePropertyChanged();

                        Enqueue($"G0 X{-Settings.Tool1Offset.X} Y{-Settings.Tool1Offset.Y} F{Settings.FastFeedRate}");
                        Enqueue("M400"); // Wait for previous command to finish before executing next one.
                        Enqueue("G4 P1"); // just pause for 1ms
                        Enqueue($"M42 P31 ${(viewType == ViewTypes.Camera ? 0 : 255)}");

                        // Wait for the all the messages to get sent out (but won't get an OK for G4 until G0 finishes)
                        System.Threading.SpinWait.SpinUntil(() => ToSendQueueCount > 0, 5000);

                        // wait until G4 gets marked at sent
                        System.Threading.SpinWait.SpinUntil(() => UnacknowledgedBytesSent == 0, 5000);

                        _viewType = ViewTypes.Tool1;
                        RaisePropertyChanged();
                        Services.DispatcherServices.Invoke(() =>
                        {
                            RaisePropertyChanged(nameof(ViewType));
                        });
                    }
                    else if (_viewType == ViewTypes.Tool1 && viewType == ViewTypes.Camera)
                    {
                        _viewType = ViewTypes.Moving;
                        Enqueue($"G0 X{Settings.Tool1Offset.X} Y{Settings.Tool1Offset.Y} F{Settings.FastFeedRate}");
                        Enqueue("M400"); // Wait for previous command to finish before executing next one.
                        Enqueue("G4 P1"); // just pause for 1ms

                        // Wait for the all the messages to get sent out (but won't get an OK for G4 until G0 finishes)
                        System.Threading.SpinWait.SpinUntil(() => ToSendQueueCount > 0, 5000);

                        // wait until G4 gets marked at sent
                        System.Threading.SpinWait.SpinUntil(() => UnacknowledgedBytesSent == 0, 5000);

                        _viewType = ViewTypes.Camera;
                        Services.DispatcherServices.Invoke(() =>
                        {
                            RaisePropertyChanged(nameof(ViewType));
                        });
                    }

                    // 4. set the machine back to absolute points
                    Enqueue("G90");

                    // 5. Set the machine location to where it was prior to the move.
                    Enqueue($"G92 X{currentLocation.X} Y{currentLocation.Y}");
                });
            }

            while (UnacknowledgedBytesSent > 0) await Task.Delay(1);
        }

        private ViewTypes _viewType = ViewTypes.Camera;
        public ViewTypes ViewType
        {
            get { return _viewType; }
            set
            {
                if (_viewType != value)
                {
                    SetViewTypeAsync(value);
                    RaisePropertyChanged();
                }
            }
        }

        void SetViewType(ViewTypes viewType)
        {
            if (_viewType != viewType)
            {
                _viewType = viewType;
                RaisePropertyChanged();
            }
        }

        private Vector3 _machinePosition = new Vector3();
        /// <summary>
        /// X,Y Position as returned from the machine.
        /// </summary>
        public Vector3 MachinePosition
        {
            get { return _machinePosition; }
            set
            {
                _machinePosition = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(WorkspacePosition));
                RaisePropertyChanged(nameof(NormalizedPosition));
            }
        }

        private Vector3 _workspacePosition = new Vector3();

        public Vector3 WorkspacePosition
        {
            get
            {
                return _workspacePosition;
            }
            set
            {
                _workspacePosition = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NormalizedPosition));
            }

        }

        private Vector3 _workPositionOffset = new Vector3();
        /// <summary>
        ///  X, Y Machine of the origin of the material
        /// </summary>
        public Vector3 WorkPositionOffset
        {
            get { return _workPositionOffset; }
            set
            {
                _workPositionOffset = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NormalizedPosition));
            }
        }

        bool? _endStopXMin = null;
        public bool? EndStopXMin
        {
            get { return _endStopXMin; }
            set
            {
                _endStopXMin = value;
                RaisePropertyChanged();
            }
        }

        bool? _endStopXMax = null;
        public bool? EndStopXMax
        {
            get { return _endStopXMax; }
            set
            {
                _endStopXMax = value;
                RaisePropertyChanged();
            }
        }

        bool? _endStopYMin = null;
        public bool? EndStopYMin
        {
            get { return _endStopYMin; }
            set
            {
                _endStopYMin = value;
                RaisePropertyChanged();
            }
        }

        bool? _endStopYMax = null;
        public bool? EndStopYMax
        {
            get { return _endStopYMax; }
            set
            {
                _endStopYMax = value;
                RaisePropertyChanged();
            }
        }

        bool? _endStopZ1Min = null;
        public bool? EndStopZ1Min
        {
            get { return _endStopZ1Min; }
            set
            {
                _endStopZ1Min = value;
                RaisePropertyChanged();
            }
        }

        bool? _endStopZ1Max = null;
        public bool? EndStopZ1Max
        {
            get { return _endStopZ1Max; }
            set
            {
                _endStopZ1Max = value;
                RaisePropertyChanged();
            }
        }

        bool? _endStopZ2Min = null;
        public bool? EndStopZ2Min
        {
            get { return _endStopZ2Min; }
            set
            {
                _endStopZ2Min = value;
                RaisePropertyChanged();
            }
        }

        bool? _endStopZ2Max = null;
        public bool? EndStopZ2Max
        {
            get { return _endStopZ2Max; }
            set
            {
                _endStopZ2Max = value;
                RaisePropertyChanged();
            }
        }


        private int _filePosition = 0;
        public int FilePosition
        {
            get { return _filePosition; }
            private set
            {
                _filePosition = value;
                RaisePropertyChanged();
            }
        }


        private OperatingMode _mode = OperatingMode.Disconnected;
        public OperatingMode Mode
        {
            get { return _mode; }
            private set
            {
                if (_mode == value)
                    return;

                _mode = value;

                RaisePropertyChanged();
            }
        }

        private string _status = "Disconnected";
        public string Status
        {
            get { return _status; }
            private set
            {
                if (_status == value)
                    return;

                _status = value;
                RaisePropertyChanged();
            }
        }

        private bool _connected = false;
        public bool Connected
        {
            get { return _connected; }
            private set
            {
                if (value == _connected)
                    return;

                _connected = value;

                if (!Connected)
                    Mode = OperatingMode.Disconnected;

                RaisePropertyChanged();
            }
        }

        private int _unacknowledgedBytesSent;
        public int UnacknowledgedBytesSent
        {
            get { return _unacknowledgedBytesSent; }
            set
            {
                if (_unacknowledgedBytesSent == value)
                    return;

                _unacknowledgedBytesSent = value;

                Busy = _unacknowledgedBytesSent > 0;

                RaisePropertyChanged();
            }
        }

        public bool HasBufferSpaceAvailableForByteCount(int bytes)
        {
            return bytes < (Settings.ControllerBufferSize - UnacknowledgedBytesSent);
        }

        public void AddStatusMessage(StatusMessageTypes type, string message, MessageVerbosityLevels verbosityLevel = MessageVerbosityLevels.Normal)
        {
            if (IsInitialized && Settings != null && verbosityLevel >= Settings.MessageVerbosity)
            {
                Services.DispatcherServices.Invoke(() =>
                {
                    Messages.Add(Models.StatusMessage.Create(type, message));
                    RaisePropertyChanged(nameof(MessageCount));
                });
            }
        }
    }
}
