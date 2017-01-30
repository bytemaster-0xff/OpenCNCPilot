﻿using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;

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
        public int BufferState
        {
            get { return _bufferState; }
            private set
            {
                if (_bufferState == value)
                    return;

                _bufferState = value;

                RaisePropertyChanged();
            }
        }        

        public bool HasJob
        {
            get { return CurrentJob != null; }
        }

        public bool BufferSpaceAvailable(int bytes)
        {
            return bytes < (_settings.ControllerBufferSize - BufferState);
        }

        public void AddStatusMessage(StatusMessageTypes type, string message)
        {
            Services.DispatcherServices.Invoke(() =>
            {
                Messages.Add(Models.StatusMessage.Create(type, message));
            });
        }
    }
}