using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using LagoVista.GCode.Sender.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender
{
    public class MachineSettings : ModelBase, INotifyPropertyChanged
    {
        public string Id { get; set; }

        public int StatusPollIntervalIdle { get; set; }
        public int StatusPollIntervalRunning { get; set; }
        public int ControllerBufferSize { get; set; }

        public double ViewportArcSplit { get; set; }
        public double ArcToLineSegmentLength { get; set; }
        public double SplitSegmentLength { get; set; }

        SerialPortInfo _currentSerialPort;
        public SerialPortInfo CurrentSerialPort
        {
            get { return _currentSerialPort; }
            set { Set(ref _currentSerialPort, value); }
        }

        ConnectionTypes _connectionType;
        public ConnectionTypes ConnectionType
        {
            get { return _connectionType; }
            set { Set(ref _connectionType, value); }
        }


        private String _ipAddress;
        public String IPAddress
        {
            get { return _ipAddress; }
            set { Set(ref _ipAddress, value); }
        }

        public bool EnableCodePreview { get; set; }
        public double ProbeSafeHeight { get; set; }
        public double ProbeMaxDepth { get; set; }
        public double ProbeMinimumHeight { get; set; }

        public bool PauseOnToolChange { get; set; }

        public double ProbeHeightMovementFeed { get; set; }

        public int ProbeTimeoutSeconds { get; set; }

        int _workAreaWidth;
        public int WorkAreaWidth
        {
            get { return _workAreaWidth; }
            set { Set(ref _workAreaWidth, value); }
        }

        int _workAreaHeight;
        public int WorkAreaHeight
        {
            get { return _workAreaHeight; }
            set { Set(ref _workAreaHeight, value); }
        }

        public bool AbortOnProbeFail { get; set; }
        public double ProbeFeed { get; set; }

        private double _xyStepSize;
        public double XYStepSize
        {
            get { return _xyStepSize; }
            set { Set(ref _xyStepSize, value); }
        }

        private double _zStepSize;
        public double ZStepSize
        {
            get { return _zStepSize; }
            set { Set(ref _zStepSize, value); }
        }

        private String _machineName;
        public String MachineName
        {
            get { return _machineName; }
            set { Set(ref _machineName, value); }
        }

        MachineOrigin _machineOrigin;
        public MachineOrigin MachineOrigin
        {
            get { return _machineOrigin; }
            set { Set(ref _machineOrigin, value); }
        }

        JogGCodeCommand _jogGCodeCommand;
        public JogGCodeCommand JogGCodeCommand
        {
            get { return _jogGCodeCommand; }
            set { Set(ref _jogGCodeCommand, value); }
        }

        MessageVerbosityLevels _messageVerbosity;
        public MessageVerbosityLevels MessageVerbosity
        {
            get { return _messageVerbosity; }
            set { Set(ref _messageVerbosity, value); }
        }

        private int _jogFeedRate;
        public int JogFeedRate
        {
            get { return _jogFeedRate; }
            set { Set(ref _jogFeedRate, value); }
        }

        StepModes _xyStepMode;
        public StepModes XYStepMode
        {
            get { return _xyStepMode; }
            set { Set(ref _xyStepMode, value); }
        }

        StepModes _zStepMode;
        public StepModes ZStepMode
        {
            get { return _zStepMode; }
            set { Set(ref _zStepMode, value); }
        }

        Camera _positioningCamera;
        public Camera PositioningCamera
        {
            get { return _positioningCamera; }
            set { Set(ref _positioningCamera, value); }
        }

        Camera _partInspectionCamera;
        public Camera PartInspectionCamera
        {
            get { return _partInspectionCamera; }
            set { Set(ref _partInspectionCamera, value); }
        }

        Point2D<double> _homeFiducialOffset;
        public Point2D<double> HomeFiducialOffset
        {
            get { return _homeFiducialOffset; }
            set { Set(ref _homeFiducialOffset, value); }
        }

        Vector3 _defaultWorkspaceOffset;
        public Vector3 WorkspaceOffset
        {
            get { return _defaultWorkspaceOffset; }
            set { Set(ref _defaultWorkspaceOffset, value); }
        }

        public FirmwareTypes MachineType { get; set; }

        private string _settingsName;

        public async static Task<MachineSettings> LoadAsync(string settingsName)
        {
            try
            {
                var settings = await Services.Storage.GetAsync<MachineSettings>("settingsName.json");
                if (settings == null)
                    settings = MachineSettings.Default;

                settings._settingsName = settingsName;

                return settings;
            }
            catch (Exception)
            {
                return MachineSettings.Default;
            }
        }

        public List<string> Validate()
        {
            var errs = new List<string>();

            if (String.IsNullOrEmpty(MachineName))
            {
                errs.Add("Machine Name is Requried.");
            }

            return errs;
        }

        public double ProbeOffset { get; set; }

        public MachineSettings Clone()
        {
            return this.MemberwiseClone() as MachineSettings;
        }

        public static MachineSettings Default
        {
            get
            {
                return new MachineSettings()
                {
                    Id = Guid.NewGuid().ToString(),
                    MachineName = "Machine 1",
                    ProbeOffset = 0.0,
                    ControllerBufferSize = 120,
                    StatusPollIntervalIdle = 1000,
                    StatusPollIntervalRunning = 100,
                    JogFeedRate = 2000,
                    ProbeTimeoutSeconds = 30,
                    MessageVerbosity = MessageVerbosityLevels.Normal,
                    MachineOrigin = MachineOrigin.Bottom_Left,
                    JogGCodeCommand = JogGCodeCommand.G0,
                    ViewportArcSplit = 1,
                    EnableCodePreview = true,
                    ProbeSafeHeight = 5,
                    ProbeMinimumHeight = 1,
                    ProbeMaxDepth = 5,
                    AbortOnProbeFail = false,
                    ProbeFeed = 20,
                    ProbeHeightMovementFeed = 1000,
                    ArcToLineSegmentLength = 1,
                    SplitSegmentLength = 5,
                    XYStepSize = 1,
                    ZStepSize = 1,
                    WorkAreaWidth = 300,
                    WorkAreaHeight = 200
                };
            }
        }

    }
}
