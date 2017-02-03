using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using System;
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

        JogGCodeCommand _jogGCodeCommand;
        public JogGCodeCommand JogGCodeCommand
        {
            get { return _jogGCodeCommand; }
            set { Set(ref _jogGCodeCommand, value); }
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
                    ControllerBufferSize = 120,
                    StatusPollIntervalIdle = 1000,
                    StatusPollIntervalRunning = 100,
                    JogFeedRate = 2000,
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
                };
            }
        }

    }
}
