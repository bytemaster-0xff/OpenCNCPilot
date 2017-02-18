using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using System;
using System.Diagnostics;

namespace LagoVista.GCode.Sender
{
    public partial class Machine
    {
        private Vector3 _machinePosition = new Vector3();
        public Vector3 MachinePosition
        {
            get { return _machinePosition; }
            set
            {
                _machinePosition = value;
                RaisePropertyChanged();
            }
        }

        private Vector3 _workPosition = new Vector3();
        public Vector3 WorkPosition
        {
            get { return _workPosition; }
            set
            {
                _workPosition = value;
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
            return bytes < (_settings.ControllerBufferSize - UnacknowledgedBytesSent);
        }

        public void AddStatusMessage(StatusMessageTypes type, string message, MessageVerbosityLevels verbosityLevel = MessageVerbosityLevels.Normal)
        {
            if (Settings != null && verbosityLevel >= Settings.MessageVerbosity)
            {
                Services.DispatcherServices.Invoke(() =>
                {
                    Debug.WriteLine($"{DateTime.Now.ToString()}  {type} - {message}");

                    Messages.Add(Models.StatusMessage.Create(type, message));
                    RaisePropertyChanged(nameof(MessageCount));
                });
            }
        }
    }
}
