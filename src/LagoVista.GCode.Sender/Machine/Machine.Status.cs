using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using System;
using System.Diagnostics;

namespace LagoVista.GCode.Sender
{
    public partial class Machine
    {

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
            if (IsInitialized &&  Settings != null && verbosityLevel >= Settings.MessageVerbosity)
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
