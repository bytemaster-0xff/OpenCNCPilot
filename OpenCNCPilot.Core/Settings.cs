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

        public double MicroStepSize { get; set; }
        public double SmallStepSize { get; set; }
        public double MediumStepSize { get; set; }
        public double LargeStepSize { get; set; }

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
            catch(Exception)
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
                    ViewportArcSplit = 1,
                    EnableCodePreview = true,
                    ProbeSafeHeight = 5,
                    ProbeMinimumHeight = 1,
                    ProbeMaxDepth = 5,
                    AbortOnProbeFail = false,
                    ProbeFeed = 20,
                    ArcToLineSegmentLength = 1,
                    SplitSegmentLength = 5,
                    MicroStepSize = 0.1,
                    SmallStepSize = 1,
                    MediumStepSize = 5,
                    LargeStepSize = 10
                };
            }
        }

    }
}
