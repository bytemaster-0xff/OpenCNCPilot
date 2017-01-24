using Newtonsoft.Json;
using OpenCNCPilot.Core.Platform;
using System;

namespace OpenCNCPilot.Core
{
    public class Settings
    {
        public int StatusPollInterval { get; set; }
        public int ControllerBufferSize { get; set; }

        public double ViewportArcSplit { get; set; }
        public double ArcToLineSegmentLength { get; set; }
        public double SplitSegmentLength { get; set; }

        public String SerialPortName { get; set; }
        public int SerialPortBaud { get; set; }


        public bool EnableCodePreview { get; set; }
        public double ProbeSafeHeight { get; set; }
        public double ProbeMaxDepth { get; set; }
        public double ProbeMinimumHeight { get; set; }
        public bool AbortOnProbeFail { get; set; }
        public double ProbeFeed { get; set; }

        public static Settings Load(IStorage storage)
        {
            var json = storage.ReadAllText("Settings.json");
            if (String.IsNullOrEmpty(json))
                return Settings.Default;

            return JsonConvert.DeserializeObject<Settings>(json);
        }

        public void Save(IStorage storage)
        {
            var json = JsonConvert.SerializeObject(this);

            storage.WriteAllText("Settings.json", json);
        }

        public static Settings Default
        {
            get
            {
                return new Settings()
                {
                    SerialPortName = "COM1",
                    SerialPortBaud = 115200,
                    ControllerBufferSize = 120,
                    StatusPollInterval = 100,
                    ViewportArcSplit = 1,
                    EnableCodePreview = true,
                    ProbeSafeHeight = 5,
                    ProbeMinimumHeight = 1,
                    ProbeMaxDepth = 5,
                    AbortOnProbeFail = false,
                    ProbeFeed = 20,
                    ArcToLineSegmentLength = 1,
                    SplitSegmentLength = 5
                };
            }
        }

    }
}
