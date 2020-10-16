using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using System;
using System.Diagnostics;

namespace LagoVista.GCode.Sender
{
    public enum ViewTypes
    {
        Camera,
        Tool1,
        Tool2,
    }

    public partial class Machine
    {
        private ViewTypes _viewType;
        public ViewTypes ViewType
        {
            get { return _viewType; }
            set
            {
                _viewType = value;
                RaisePropertyChanged();
                switch (value)
                {
                    case ViewTypes.Camera: Enqueue("M50"); break;
                    case ViewTypes.Tool1: Enqueue("M51"); break;
                    case ViewTypes.Tool2: Enqueue("M51"); break;
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
            }
        }

        private Vector3 _workspacePosition = new Vector3();

        public Vector3 WorkspacePosition
        {
            get
            {
                if (Settings.MachineType == FirmwareTypes.Repeteir_PnP)
                {
                    return MachinePosition - Settings.WorkspaceOffset;
                }
                else
                {
                    return _workspacePosition;
                }
            }
            set
            {
                if (Settings.MachineType == FirmwareTypes.Repeteir_PnP)
                {
                    /* nop, always calculated on client. */
                }
                else
                {
                    _workspacePosition = value;
                }
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

        private int _bufferState;
        public int UnacknowledgedBytesSent
        {
            get { return _bufferState; }
            set
            {
                if (_bufferState == value)
                    return;

                _bufferState = value;

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
