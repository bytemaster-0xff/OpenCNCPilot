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
using System.Collections.Generic;

namespace LagoVista.GCode.Sender
{
    public partial class Machine : IMachine
    {
        CancellationToken _cancelToken;

        MachinesRepo _machineRepo;

        public event PropertyChangedEventHandler PropertyChanged;

        public Machine(MachinesRepo repo)
        {
            _machineRepo = repo;

            Messages = new System.Collections.ObjectModel.ObservableCollection<Models.StatusMessage>();
            AddStatusMessage(StatusMessageTypes.Info, "Startup.");

            ToolChangeManager = new Managers.ToolChangeManager(this, Core.PlatformSupport.Services.Logger);
            GCodeFileManager = new Managers.GCodeFileManager(this, Core.PlatformSupport.Services.Logger, ToolChangeManager);
            BoardManager = new Managers.BoardManager(this, Core.PlatformSupport.Services.Logger);
            HeightMapManager = new Managers.HeightMapManager(this, Core.PlatformSupport.Services.Logger, BoardManager);
            ProbingManager = new Managers.ProbingManager(this, Core.PlatformSupport.Services.Logger);
            MachineVisionManager = new Managers.MachineVisionManager(this, Core.PlatformSupport.Services.Logger, BoardManager);
        }

        public Task InitAsync()
        {
            IsInitialized = true;

            return Task.FromResult(default(object));
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

        public bool CanSetMode(OperatingMode mode)
        {
            throw new NotImplementedException();
        }

        public bool SetMode(OperatingMode mode)
        {
            Mode = mode;
            return true;
        }
    }
}
