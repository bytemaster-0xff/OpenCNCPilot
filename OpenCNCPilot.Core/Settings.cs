using OpenCNCPilot.Core.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        public void Load(IStorage storage)
        {

        }

        public void Save(IStorage storage)
        {

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
