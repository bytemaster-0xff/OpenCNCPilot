using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.GCode.Sender.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender
{
    public class Settings : ModelBase, INotifyPropertyChanged
    {
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

        public bool EnableCodePreview { get; set; }
        public double ProbeSafeHeight { get; set; }
        public double ProbeMaxDepth { get; set; }
        public double ProbeMinimumHeight { get; set; }

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

        public FirmwareTypes MachineType { get; set; }

        public async static Task<Settings> LoadAsync()
        {
            try
            {
                var settings = await Services.Storage.GetAsync<Settings>("Settings.json");
                if (settings == null)
                    settings = Settings.Default;
                return settings;
            }
            catch (Exception)
            {
                return Settings.Default;
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

        public async Task SaveAsync()
        {
            await Services.Storage.StoreAsync(this, "Settings.json");
        }

        public static Settings Default
        {
            get
            {
                return new Settings()
                {
                    MachineName = "",
                    ProbeOffset = 0.0,
                    ControllerBufferSize = 120,
                    StatusPollIntervalIdle = 1000,
                    StatusPollIntervalRunning = 100,
                    JogFeedRate = 2000,
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
