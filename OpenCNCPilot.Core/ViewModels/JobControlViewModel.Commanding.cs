using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class JobControlViewModel
    {
        private void InitCommands()
        {
            SendGCodeFileCommand = new RelayCommand(SendGCodeFile, CanSendGcodeFile);
            StartProbeCommand = new RelayCommand(StartProbe, CanProbe);
            StartProbeHeightMapCommand = new RelayCommand(StartHeightMap, CanProbeHeightMap);
            StopCommand = new RelayCommand(StopJob, CanStopJob);
            PauseCommand = new RelayCommand(PauseJob, CanPauseJob);

            ConnectCommand = new RelayCommand(Connect, CanChangeConnectionStatus);

            EmergencyStopCommand = new RelayCommand(EmergencyStop, CanSendEmergencyStop);

            Machine.PropertyChanged += _machine_PropertyChanged;
            Machine.HeightMapManager.PropertyChanged += HeightMapManager_PropertyChanged;
        }

        private void RefreshCommandExecuteStatus()
        {
            ConnectCommand.RaiseCanExecuteChanged();

            SendGCodeFileCommand.RaiseCanExecuteChanged();
            StartProbeCommand.RaiseCanExecuteChanged();
            StartProbeHeightMapCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
            PauseCommand.RaiseCanExecuteChanged();

            EmergencyStopCommand.RaiseCanExecuteChanged();
        }

        private void HeightMapManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.HeightMapManager.HeightMap) ||
               e.PropertyName == nameof(Machine.HeightMapManager.HeightMap.Status))
            {
                DispatcherServices.Invoke(RefreshCommandExecuteStatus);
            }
        }

        private void _machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.IsInitialized) ||
                e.PropertyName == nameof(Machine.Mode) ||
                e.PropertyName == nameof(Machine.GCodeFileManager.HasValidFile) ||
                e.PropertyName == nameof(Machine.Connected))
            {
                DispatcherServices.Invoke(RefreshCommandExecuteStatus);
            }
        }

        public bool CanChangeConnectionStatus()
        {
            return Machine.IsInitialized && Machine.Settings.CurrentSerialPort != null && Machine.Settings.CurrentSerialPort.Id != "empty";
        }

        public bool CanSendGcodeFile()
        {
            return Machine.IsInitialized && 
                Machine.GCodeFileManager.HasValidFile &&
                Machine.Connected &&
                Machine.Mode == OperatingMode.Manual;
        }

        public bool CanPauseJob()
        {
            return Machine.IsInitialized && 
                Machine.Mode == OperatingMode.SendingGCodeFile ||
                Machine.Mode == OperatingMode.ProbingHeightMap ||
                Machine.Mode == OperatingMode.ProbingHeight;
        }

        public bool CanProbeHeightMap()
        {
            return Machine.IsInitialized && 
                Machine.Connected
                && Machine.Mode == OperatingMode.Manual
                && Machine.HeightMapManager.HasHeightMap;
        }

        public bool CanProbe()
        {
            return Machine.IsInitialized && 
                Machine.Connected
                && Machine.Mode == OperatingMode.Manual;
        }

        public bool CanStopJob()
        {
            return Machine.IsInitialized && 
                Machine.Mode == OperatingMode.SendingGCodeFile ||
                Machine.Mode == OperatingMode.ProbingHeightMap ||
                Machine.Mode == OperatingMode.ProbingHeight;
        }

        public bool CanSendEmergencyStop()
        {
            return Machine.IsInitialized && Machine.Connected;
        }

        public RelayCommand StopCommand { get; private set; }

        public RelayCommand PauseCommand { get; private set; }

        public RelayCommand StartProbeCommand { get; private set; }
        public RelayCommand StartProbeHeightMapCommand { get; private set; }
        public RelayCommand SendGCodeFileCommand { get; private set; }
        public RelayCommand EmergencyStopCommand { get; private set; }

        public RelayCommand ConnectCommand { get; private set; }

    }
}
