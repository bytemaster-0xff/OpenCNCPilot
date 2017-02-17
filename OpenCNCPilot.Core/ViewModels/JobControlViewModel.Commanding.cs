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
            StartJobCommand = new RelayCommand(StartJob, CanStartJob);
            StartProbeJobCommand = new RelayCommand(StartProbeJob, CanProbeHeightMap);
            StartProbeHeightMapCommand = new RelayCommand(StartProbeHeight, CanStartProbeHeight);
            StopJobCommand = new RelayCommand(StopJob, CanStopJob);

            ConnectCommand = new RelayCommand(Connect, CanChangeConnectionStatus);

            PauseJobCommand = new RelayCommand(PauseJob, CanPauseJob);

            EmergencyStopCommand = new RelayCommand(Kill, CanKill);

            Machine.PropertyChanged += _machine_PropertyChanged;
            Machine.HeightMapManager.PropertyChanged += HeightMapProbingManager_PropertyChanged; 
        }

        private void RefreshCommandExecuteStatus()
        {
            ConnectCommand.RaiseCanExecuteChanged();
            StartJobCommand.RaiseCanExecuteChanged();
            StartProbeJobCommand.RaiseCanExecuteChanged();
            StartProbeHeightMapCommand.RaiseCanExecuteChanged();
            StopJobCommand.RaiseCanExecuteChanged();
            PauseJobCommand.RaiseCanExecuteChanged();
            EmergencyStopCommand.RaiseCanExecuteChanged();
        }

        private void HeightMapProbingManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(Machine.HeightMapManager.HeightMap) ||
               e.PropertyName == nameof(Machine.HeightMapManager.HeightMap.Status))
            {
                DispatcherServices.Invoke(RefreshCommandExecuteStatus); 
            }
        }

        private void _machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.Mode) ||
                e.PropertyName == nameof(Machine.JobManager.HasValidFile) ||
                e.PropertyName == nameof(Machine.IsInitialized) ||
                e.PropertyName == nameof(Machine.Connected))
            {
                DispatcherServices.Invoke(RefreshCommandExecuteStatus);
            }
        }

        public bool CanChangeConnectionStatus()
        {
            return Machine.IsInitialized &&  Machine.Settings.CurrentSerialPort != null && Machine.Settings.CurrentSerialPort.Id != "empty";
        }

        public bool CanStartJob()
        {
            return Machine.JobManager.HasValidFile &&
                Machine.Connected &&
                Machine.Mode == OperatingMode.Manual;
        }

        public bool CanPauseJob()
        {
            return Machine.Mode == OperatingMode.SendingJob ||
                Machine.Mode == OperatingMode.ProbingHeightMap ||
                Machine.Mode == OperatingMode.ProbingHeight;
        }

        public bool CanProbeHeightMap()
        {
            return Machine.Connected
                && Machine.Mode == OperatingMode.Manual 
                && Machine.HeightMapManager.HeightMap != null;
        }

        public bool CanStartProbeHeight()
        {
            return Machine.Connected
                && Machine.Mode == OperatingMode.Manual;
        }

        public bool CanStopJob()
        {
            return Machine.Mode == OperatingMode.SendingJob ||
                Machine.Mode == OperatingMode.ProbingHeightMap ||
                Machine.Mode == OperatingMode.ProbingHeight;
        }

        public bool CanKill()
        {
            return Machine.Connected;
        }

        public RelayCommand StopJobCommand { get; private set; }

        public RelayCommand PauseJobCommand { get; private set; }

        public RelayCommand StartProbeJobCommand { get; private set; }
        public RelayCommand StartProbeHeightMapCommand { get; private set; }
        public RelayCommand StartJobCommand { get; private set; }
        public RelayCommand EmergencyStopCommand { get; private set; }

        public RelayCommand ConnectCommand { get; private set; }

    }
}
