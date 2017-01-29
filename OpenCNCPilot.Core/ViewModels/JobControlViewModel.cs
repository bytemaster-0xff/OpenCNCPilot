using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using LagoVista.Core.ViewModels;
using OpenCNCPilot.Core.Communication;
using System;

namespace OpenCNCPilot.Core.ViewModels
{
    public class JobControlViewModel : ViewModelBase
    {
        IMachine _machine;
        Settings _settings;
        public JobControlViewModel(IMachine machine, Settings settings)
        {
            _machine = machine;
            _settings = settings;

            StartJobCommand = new RelayCommand(StartJob, CanStartJob);
            StartProbeJobCommand = new RelayCommand(StartProbeJob, CanProbeHeightMap);
            StartProbeHeightMapCommand = new RelayCommand(StartProbeHeight, CanStartProbeHeight);
            StopJobCommand = new RelayCommand(StopJob, CanStopJob);

            ConnectCommand = new RelayCommand(Connect, CanChangeConnectionStatus);

            PauseJobCommand = new RelayCommand(PauseJob, CanPauseJob);

            EmergencyStopCommand = new RelayCommand(Kill, CanKill);

            _machine.PropertyChanged += _machine_PropertyChanged;
            _settings.PropertyChanged += _settings_PropertyChanged;
        }

        private void _machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_machine.Mode) ||
                e.PropertyName == nameof(_machine.File) ||
                e.PropertyName == nameof(_machine.Connected))
            {
                DispatcherServices.Invoke(() =>
                {
                    StartJobCommand.RaiseCanExecuteChanged();
                    StartProbeJobCommand.RaiseCanExecuteChanged();
                    StartProbeHeightMapCommand.RaiseCanExecuteChanged();
                    StopJobCommand.RaiseCanExecuteChanged();
                    PauseJobCommand.RaiseCanExecuteChanged();
                    EmergencyStopCommand.RaiseCanExecuteChanged();
                });
            }
        }

        private void _settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(_settings.CurrentSerialPort))
            {
                ConnectCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanChangeConnectionStatus()
        {
            return _settings.CurrentSerialPort != null && _settings.CurrentSerialPort.Id != "empty";
        }

        public bool CanStartJob()
        {
            return _machine.File != null && 
                _machine.Connected && 
                _machine.Mode == Communication.Machine.OperatingMode.Manual;
        }

        public bool CanPauseJob()
        {
            return _machine.Mode == Communication.Machine.OperatingMode.SendingJob ||
                _machine.Mode == Communication.Machine.OperatingMode.ProbingHeightMap ||
                _machine.Mode == Communication.Machine.OperatingMode.ProbingHeight;
        }

        public bool CanProbeHeightMap()
        {
            return _machine.File != null
                && _machine.Connected
                && _machine.Mode == Communication.Machine.OperatingMode.Manual;
        }

        public bool CanStartProbeHeight()
        {
            return _machine.File != null
                && _machine.Connected
                && _machine.Mode == Communication.Machine.OperatingMode.Manual;
        }

        public bool CanStopJob()
        {
            return _machine.Mode == Communication.Machine.OperatingMode.SendingJob || 
                _machine.Mode == Communication.Machine.OperatingMode.ProbingHeightMap || 
                _machine.Mode == Communication.Machine.OperatingMode.ProbingHeight;
        }

        public bool CanKill()
        {
            return _machine.Connected;
        }

        public void Kill()
        {

        }

        public void StopJob()
        {

        }


        public void StartProbeJob()
        {

        }

        public void StartProbeHeight()
        {

        }

        public async void Connect()
        {
            if (_machine.Connected)
            {
                await _machine.DisconnectAsync();
            }
            else
            {
                if (_settings.CurrentSerialPort.Name == "Simulated")
                {
                    await _machine.ConnectAsync(new SimulatedMachine(_settings.MachineType));
                }
                else
                {
                    var port = DeviceManager.CreateSerialPort(_settings.CurrentSerialPort);
                    var stream = await port.OpenAsync();
                    await _machine.ConnectAsync(port);
                }
            }
        }


        public IMachine Machine{ get { return _machine;} }

        public void StartJob()
        {
            _machine.FileStart();
        }

        public void PauseJob()
        {
            switch(_machine.Mode)
            {
                case Communication.Machine.OperatingMode.SendingJob: _machine.FilePause(); break;
            }
           
        }


        public bool IsCreatingHeightMap { get { return _machine.Mode == Communication.Machine.OperatingMode.ProbingHeightMap; } }
        public bool IsProbingHeight { get { return _machine.Mode == Communication.Machine.OperatingMode.ProbingHeight; } }
        public bool IsRunningJob { get { return _machine.Mode == Communication.Machine.OperatingMode.SendingJob; } }
        public RelayCommand StopJobCommand { get; private set; }

        public RelayCommand PauseJobCommand { get; private set; }    
    
        public RelayCommand StartProbeJobCommand { get; private set; }
        public RelayCommand StartProbeHeightMapCommand { get; private set; }
        public RelayCommand StartJobCommand { get; private set; }
        public RelayCommand EmergencyStopCommand { get; private set; }

        public RelayCommand ConnectCommand { get; private set; }
    }
}
