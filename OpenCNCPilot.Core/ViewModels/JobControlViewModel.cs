using LagoVista.Core.Commanding;
using OpenCNCPilot.Core.Communication;
using System;

namespace OpenCNCPilot.Core.ViewModels
{
    public class JobControlViewModel
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

            ConnectCommand = new RelayCommand(Connect);

            PauseJobCommand = new RelayCommand(PauseJob, CanPauseJob);

            _machine.PropertyChanged += _machine_PropertyChanged;
        }

        public bool CanStartJob(Object pararm)
        {
            return _machine.File != null && _machine.Connected && _machine.Mode == Communication.Machine.OperatingMode.Manual;
        }

        public bool CanPauseJob(Object pararm)
        {
            return _machine.File != null && _machine.Connected && _machine.Mode == Communication.Machine.OperatingMode.ProbingHeightMap || _machine.Mode == Communication.Machine.OperatingMode.SendingJob;
        }

        public bool CanProbeHeightMap(Object pararm)
        {
            return _machine.File != null && _machine.Connected && _machine.Mode == Communication.Machine.OperatingMode.ProbingHeightMap || _machine.Mode == Communication.Machine.OperatingMode.ProbingHeightMap;
        }

        public bool CanStartProbeHeight(Object pararm)
        {
            return _machine.File != null && _machine.Connected && _machine.Mode == Communication.Machine.OperatingMode.ProbingHeightMap || _machine.Mode == Communication.Machine.OperatingMode.ProbingHeight;
        }


        public void StartProbeJob(Object param)
        {

        }

        public void StartProbeHeight(Object param)
        {

        }

        public void Connect()
        {
            if (_settings.CurrentSerialPort.Name == "Simulated")
            {
                _machine.Connect(new SimulatedGCodeMachine());
            }
            else
            {
                //var serialPort = new LagoVista.Core.Wp SerialPort(App.Current.Settings.CurrentSerialPort.Id, App.Current.Settings.CurrentSerialPort.BaudRate);
                //serialPort.Open();
                //_machine.Connect(serialPort.BaseStream);
            }
        }


        public IMachine Machine{ get { return _machine;} }

        public void StartJob(Object pararm)
        {
            _machine.FileStart();
        }

        public void PauseJob(Object pararm)
        {
            switch(_machine.Mode)
            {
                case Communication.Machine.OperatingMode.SendingJob: _machine.FilePause(); break;
            }
           
        }

        private void _machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_machine.Mode) || 
                e.PropertyName == nameof(_machine.File) ||
                e.PropertyName == nameof(_machine.Connected))
            {
                StartJobCommand.RaiseCanExecuteChanged();
                PauseJobCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsCreatingHeightMap { get { return _machine.Mode == Communication.Machine.OperatingMode.ProbingHeightMap; } }
        public bool IsProbingHeight { get { return _machine.Mode == Communication.Machine.OperatingMode.ProbingHeight; } }
        public bool IsRunningJob { get { return _machine.Mode == Communication.Machine.OperatingMode.SendingJob; } }

        public RelayCommand PauseJobCommand { get; private set; }    
    
        public RelayCommand StartProbeJobCommand { get; private set; }
        public RelayCommand StartProbeHeightMapCommand { get; private set; }
        public RelayCommand StartJobCommand { get; private set; }

        public RelayCommand ConnectCommand { get; private set; }
    }
}
