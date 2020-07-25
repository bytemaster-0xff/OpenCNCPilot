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

            HomingCycleCommand = new RelayCommand(HomingCycle, CanHomeAndReset);
            SoftResetCommand = new RelayCommand(SoftReset, CanHomeAndReset);
            FeedHoldCommand = new RelayCommand(FeedHold, CanPauseFeed);
            CycleStartCommand = new RelayCommand(CycleStart, CanResumeFeed);

            ClearAlarmCommand = new RelayCommand(ClearAlarm, CanClearAlarm);

            ConnectCommand = new RelayCommand(Connect, CanChangeConnectionStatus);

            EmergencyStopCommand = new RelayCommand(EmergencyStop, CanSendEmergencyStop);

            LaserOnCommand = new RelayCommand(LaserOn, CanManipulateLaser);
            LaserOffCommand = new RelayCommand(LaserOff, CanManipulateLaser);

            SpindleOnCommand = new RelayCommand(SpindleOn, CanManipulateSpindle);
            SpindleOffCommand = new RelayCommand(SpindleOff, CanManipulateSpindle);

            GotoFavorite1Command = new RelayCommand(GotoFavorite1, CanMove);
            GotoFavorite2Command = new RelayCommand(GotoFavorite2, CanMove);
            SetFavorite1Command = new RelayCommand(SetFavorite1, CanMove);
            SetFavorite2Command = new RelayCommand(SetFavorite2, CanMove);

            GotoWorkspaceHomeCommand = new RelayCommand(GotoWorkspaceHome, CanMove);
            GotoFiducialHomeCommand = new RelayCommand(GotoFiducialHome, CanGotoFiducialHome);

            Machine.PropertyChanged += _machine_PropertyChanged;
            Machine.Settings.PropertyChanged += _machine_PropertyChanged;
            Machine.HeightMapManager.PropertyChanged += HeightMapManager_PropertyChanged;
            Machine.GCodeFileManager.PropertyChanged += GCodeFileManager_PropertyChanged;
        }

        private void GCodeFileManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.GCodeFileManager.HasValidFile))
            {
                DispatcherServices.Invoke(RefreshCommandExecuteStatus);
            }
        }

        private void RefreshCommandExecuteStatus()
        {
            ConnectCommand.RaiseCanExecuteChanged();

            ClearAlarmCommand.RaiseCanExecuteChanged();

            SendGCodeFileCommand.RaiseCanExecuteChanged();
            StartProbeCommand.RaiseCanExecuteChanged();
            StartProbeHeightMapCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
            PauseCommand.RaiseCanExecuteChanged();

            SoftResetCommand.RaiseCanExecuteChanged();
            HomingCycleCommand.RaiseCanExecuteChanged();
            FeedHoldCommand.RaiseCanExecuteChanged();
            CycleStartCommand.RaiseCanExecuteChanged();
            GotoFiducialHomeCommand.RaiseCanExecuteChanged();

            EmergencyStopCommand.RaiseCanExecuteChanged();

            LaserOnCommand.RaiseCanExecuteChanged();
            LaserOffCommand.RaiseCanExecuteChanged();
            SpindleOnCommand.RaiseCanExecuteChanged();
            SpindleOffCommand.RaiseCanExecuteChanged();

            GotoFavorite1Command.RaiseCanExecuteChanged();
            GotoFavorite2Command.RaiseCanExecuteChanged();
            SetFavorite1Command.RaiseCanExecuteChanged();
            SetFavorite2Command.RaiseCanExecuteChanged();
            GotoWorkspaceHomeCommand.RaiseCanExecuteChanged();
        }

        public bool CanManipulateLaser()
        {
            return Machine.IsInitialized &&
                Machine.Settings.MachineType == FirmwareTypes.Marlin_Laser &&
                Machine.Connected &&
                Machine.Mode == OperatingMode.Manual;
        }

        public bool CanManipulateSpindle()
        {
            return Machine.IsInitialized &&
                Machine.Settings.MachineType == FirmwareTypes.GRBL1_1 &&
                Machine.Connected &&
                Machine.Mode == OperatingMode.Manual;
        }

        public bool CanMove()
        {
            return Machine.IsInitialized &&
                Machine.Connected &&
                Machine.Mode == OperatingMode.Manual;
        }

        public bool CanMoveToWorkspaceHome()
        {
            return Machine.IsInitialized &&
          Machine.Settings.MachineType == FirmwareTypes.GRBL1_1 &&
          Machine.Connected &&
          Machine.Mode == OperatingMode.Manual;
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
                e.PropertyName == nameof(Machine.Settings) ||
                e.PropertyName == nameof(Machine.Status) ||
                e.PropertyName == nameof(Machine.Connected) ||
                e.PropertyName == nameof(Machine.Settings.CurrentSerialPort))
            {
                DispatcherServices.Invoke(RefreshCommandExecuteStatus);
            }
        }

        public bool CanChangeConnectionStatus()
        {
            return Machine.IsInitialized &&
                (
                (Machine.Settings.ConnectionType == ConnectionTypes.Serial_Port && Machine.Settings.CurrentSerialPort != null && Machine.Settings.CurrentSerialPort.Id != "empty")
                || (Machine.Settings.ConnectionType == ConnectionTypes.Network && !String.IsNullOrEmpty(Machine.Settings.IPAddress))
                );
        }

        public bool CanHomeAndReset()
        {
            return Machine.IsInitialized && Machine.Connected;
        }

        public bool CanSendGcodeFile()
        {
            return Machine.IsInitialized &&
                Machine.GCodeFileManager.HasValidFile &&
                Machine.Connected &&
                Machine.Mode == OperatingMode.Manual;
        }

        public bool CanClearAlarm()
        {
            return Machine.IsInitialized &&
                   Machine.Connected &&
                   Machine.Status.ToLower() == "alarm";
        }

        public bool FavoritesAvailable()
        {
            return Machine.IsInitialized &&
                Machine.Connected
                && Machine.Mode == OperatingMode.Manual;
        }

        public bool CanPauseJob()
        {
            return Machine.IsInitialized &&
                Machine.Mode == OperatingMode.SendingGCodeFile ||
                Machine.Mode == OperatingMode.ProbingHeightMap ||
                Machine.Mode == OperatingMode.ProbingHeight;
        }

        public bool CanGotoFiducialHome()
        {
            return Machine.IsInitialized &&
                    Machine.Connected &&
                    Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP &&
                    Machine.Mode == OperatingMode.Manual;
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

        public bool CanPauseFeed()
        {
            return (Machine.IsInitialized &&
                        Machine.Connected &&
                        Machine.Status != "Hold");
        }

        public bool CanResumeFeed()
        {
            return (Machine.IsInitialized &&
                        Machine.Connected &&
                        Machine.Status == "Hold");
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


        public RelayCommand HomingCycleCommand { get; private set; }
        public RelayCommand SoftResetCommand { get; private set; }

        public RelayCommand CycleStartCommand { get; private set; }
        public RelayCommand FeedHoldCommand { get; private set; }


        public RelayCommand StartProbeCommand { get; private set; }
        public RelayCommand StartProbeHeightMapCommand { get; private set; }
        public RelayCommand SendGCodeFileCommand { get; private set; }
        public RelayCommand EmergencyStopCommand { get; private set; }

        public RelayCommand ClearAlarmCommand { get; private set; }

        public RelayCommand ConnectCommand { get; private set; }

        public RelayCommand SetFavorite1Command { get; private set; }
        public RelayCommand SetFavorite2Command { get; private set; }
        public RelayCommand GotoFavorite1Command { get; private set; }
        public RelayCommand GotoFavorite2Command { get; private set; }

        public RelayCommand GotoWorkspaceHomeCommand { get; private set; }
        public RelayCommand GotoFiducialHomeCommand { get; private set; }
        public RelayCommand LaserOnCommand { get; private set; }
        public RelayCommand LaserOffCommand { get; private set; }
        public RelayCommand SpindleOnCommand { get; private set; }
        public RelayCommand SpindleOffCommand { get; private set; }

        public RelayCommand ExhaustSolenoidCommand { get; private set; }

        public RelayCommand SuctionSolenoidCommand { get; private set;}
    }
}
