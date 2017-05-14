using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender
{
    public partial class Machine
    {
        private IGCodeFileManager _gcodeFileManager;
        public IGCodeFileManager GCodeFileManager
        {
            get { return _gcodeFileManager; }
            set
            {
                _gcodeFileManager = value;
                RaisePropertyChanged();
            }
        }

        IHeightMapManager _heightMapManager;
        public IHeightMapManager HeightMapManager
        {
            get { return _heightMapManager; }
            private set
            {
                _heightMapManager = value;
                RaisePropertyChanged();
            }
        }

        IProbingManager _probingManager;
        public IProbingManager ProbingManager
        {
            get { return _probingManager; }
            private set
            {
                _probingManager = value;
                RaisePropertyChanged();
            }
        }

        public bool IsPnPMachine
        {
            get { return Settings.MachineType == FirmwareTypes.LagoVista_PnP; }
        }

        IPCBManager _pcbManager;
        public IPCBManager PCBManager
        {
            get { return _pcbManager; }
            private set
            {
                _pcbManager = value;
                RaisePropertyChanged();
            }
        }

        IToolChangeManager _toolChangeManager;
        public IToolChangeManager ToolChangeManager
        {
            get { return _toolChangeManager; }
            private set
            {
                _toolChangeManager = value;
                RaisePropertyChanged();
            }
        }

        public Core.Models.Drawing.Vector3 NormalizedPosition
        {
            get { return MachinePosition - WorkPositionOffset; }
        }

        /* Normalized is height above PCB */
        public double Tool0Normalized
        {
            get { return _tool0 - _tool0Offset; }
        }


        public double Tool1Normalized
        {
            get { return _tool1 - _tool1Offset; }
        }

        public double Tool2Normalized
        {
            get { return _tool2 - _tool2Offset; }
        }

        private double _tool0;
        public double Tool0
        {
            get { return _tool0; }
            set
            {
                if (_tool0 != value)
                {
                    _tool0 = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Tool0Normalized));
                }
            }
        }


        private double _tool1;
        public double Tool1
        {
            get { return _tool1; }
            set
            {
                if (_tool1 != value)
                {
                    _tool1 = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Tool1Normalized));
                }
            }
        }

        private double _tool2;
        public double Tool2
        {
            get { return _tool2; }
            set
            {
                if (_tool2 != value)
                {
                    _tool2 = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Tool2Normalized));
                }
            }
        }

        /* Offset is from axis home to top of PCB */
        private double _tool0Offset;
        public double Tool0Offset
        {
            get { return _tool0Offset; }
            set
            {
                if (_tool0Offset != value)
                {
                    _tool0Offset = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Tool0Normalized));
                }
            }
        }


        private double _tool1Offset;
        public double Tool1Offset
        {
            get { return _tool1Offset; }
            set
            {
                if (_tool1Offset != value)
                {
                    _tool1Offset = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Tool1Normalized));
                }
            }
        }

        private double _tool2Offset;
        public double Tool2Offset
        {
            get { return _tool2Offset; }
            set
            {
                if (_tool2Offset != value)
                {
                    _tool2Offset = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Tool2Normalized));
                }
            }
        }


        bool _isInitialized = false;
        public bool IsInitialized
        {
            get { return _isInitialized; }
            private set
            {
                _isInitialized = value;
                RaisePropertyChanged();
            }
        }

        IMachineVisionManager _machineVisionManager;
        public IMachineVisionManager MachineVisionManager
        {
            get { return _machineVisionManager; }
            private set
            {
                _machineVisionManager = value;
                RaisePropertyChanged();
            }
        }


        IBoardAlignmentManager _boardAlignmentManager;
        public IBoardAlignmentManager BoardAlignmentManager
        {
            get { return _boardAlignmentManager; }
            private set
            {
                _boardAlignmentManager = value;
                RaisePropertyChanged();
            }
        }

        ObservableCollection<Models.StatusMessage> _messages;
        public ObservableCollection<Models.StatusMessage> Messages
        {
            get { return _messages; }
            private set
            {
                _messages = value;
                RaisePropertyChanged();
            }
        }

        private bool _motorsEnabled;
        public bool MotorsEnabled
        {
            get { return _motorsEnabled; }
            set
            {
                _motorsEnabled = value;
                SendCommand(value ? "M17" : "M18");
            }
        }

        public int MessageCount
        {
            get
            {
                if (Messages == null)
                {
                    return 0;
                }

                return Messages.Count - 1;
            }
        }

        public MachinesRepo MachineRepo
        {
            get { return _machineRepo; }
        }

        MachineSettings _settings;
        public MachineSettings Settings
        {
            get { return _settings; }
            set
            {
                _settings = value;
                _machineRepo.CurrentMachineId = value.Id;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsPnPMachine));
            }
        }
    }
}
