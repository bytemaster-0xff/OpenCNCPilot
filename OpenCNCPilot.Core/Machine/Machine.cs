using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Runtime.CompilerServices;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.GCode;
using System.ComponentModel;
using LagoVista.GCode.Sender.Util;
using System.Threading.Tasks;
using LagoVista.GCode.Sender.Interfaces;

namespace LagoVista.GCode.Sender
{
    public partial class Machine : IMachine
    {
        Settings _settings;
        CancellationToken _cancelToken;

        public event Action<Vector3, bool> ProbeFinished;

        public event PropertyChangedEventHandler PropertyChanged;

        public Machine()
        {
            Messages = new System.Collections.ObjectModel.ObservableCollection<Models.StatusMessage>();
            AddStatusMessage(StatusMessageTypes.Info, "Startup.");

            /* Have defaults loaded until the real settings come in */
            _settings = Settings.Default;
        }

        public async Task InitAsync()
        {
            _settings = await Settings.LoadAsync();
            RaisePropertyChanged(nameof(Settings));
            IsInitialized = true;
        }

        public void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            Services.DispatcherServices.Invoke(() =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName))
            );
        }

        public void ClearQueue()
        {
            if (Mode != OperatingMode.Manual)
            {
                AddStatusMessage(StatusMessageTypes.Info, "Not in manual mode.");
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
        /// </summary
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
