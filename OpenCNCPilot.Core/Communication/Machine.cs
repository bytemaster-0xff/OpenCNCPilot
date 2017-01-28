using OpenCNCPilot.Core.GCode;
using OpenCNCPilot.Core.GCode.GCodeCommands;
using OpenCNCPilot.Core.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LagoVista.Core.PlatformSupport;

namespace OpenCNCPilot.Core.Communication
{
    enum ConnectionType
    {
        Serial
    }

    public partial class Machine : IMachine
    {
        Settings _settings;
        CancellationToken _cancelToken;

        public event Action<Vector3, bool> ProbeFinished;
        public event Action<string> NonFatalException;
        public event Action<string> Info;
        public event Action<string> LineReceived;
        public event Action<string> LineSent;
        public event Action ConnectionStateChanged;
        public event Action PositionUpdateReceived;
        public event Action StatusChanged;
        public event Action DistanceModeChanged;
        public event Action UnitChanged;
        public event Action PlaneChanged;
        public event Action BufferStateChanged;
        public event Action OperatingModeChanged;
        public event Action FileChanged;
        public event Action FilePositionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public Machine(Settings settings)
        {
            _settings = settings;
        }

        public void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ClearQueue()
        {
            if (Mode != OperatingMode.Manual)
            {
                RaiseEvent(Info, "Not in Manual mode");
                return;
            }

            lock (_toSend)
            {
                _toSend.Clear();
            }
        }

        //TODO: Removed Compiled Option
        private static Regex GCodeSplitter = new Regex(@"(G)\s*(\-?\d+\.?\d*)");

        /// <summary>
        /// Updates Status info from each line sent
        /// </summary>
        /// <param name="line"></param>
        private void UpdateStatus(string line)
        {
            if (!Connected)
                return;

            //we use a Regex here so G91.1 etc don't get recognized as G91

            foreach (Match m in GCodeSplitter.Matches(line))
            {
                if (m.Groups[1].Value != "G")
                    continue;

                float code = float.Parse(m.Groups[2].Value);

                if (code == 17)
                    Plane = ArcPlane.XY;
                if (code == 18)
                    Plane = ArcPlane.YZ;
                if (code == 19)
                    Plane = ArcPlane.ZX;

                if (code == 20)
                    Unit = ParseUnit.Imperial;
                if (code == 21)
                    Unit = ParseUnit.Metric;

                if (code == 90)
                    DistanceMode = ParseDistanceMode.Absolute;
                if (code == 91)
                    DistanceMode = ParseDistanceMode.Incremental;
            }
        }

 

        /// <summary>
        /// Reports error. This is there to offload the ExpandError function from the "Real-Time" worker thread to the application thread
        /// </summary>
        private void ReportError(string error)
        {
            if (NonFatalException != null)
                NonFatalException.Invoke(GrblErrorProvider.Instance.ExpandError(error));
        }

        private void RaiseEvent(Action<string> action, string param)
        {
            if (action == null)
                return;

            Services.DispatcherServices.Invoke(() =>
            {
                action(param);
            });
        }

        private void RaiseEvent(Action action)
        {
            if (action == null)
                return;

            Services.DispatcherServices.Invoke(action);
        }
    }
}
