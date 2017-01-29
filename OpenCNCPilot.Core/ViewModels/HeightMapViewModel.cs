using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.ViewModels;
using LagoVista.GCode.Sender.Models;
using System;

namespace LagoVista.GCode.Sender.ViewModels
{
    public class HeightMapViewModel : ViewModelBase
    {
        IMachine _machine;
        Settings _settings;
        HeightMap _currentHeightMap;

        public  HeightMapViewModel(IMachine machine, Settings settings)
        {
            _machine = machine;
            _settings = settings;

            PauseProbingCommand = new RelayCommand(PauseProbing, CanPauseProbing);
            StartProbingCommand = new RelayCommand(StartProbing, CanStartProbing);

            _machine.ProbeFinished += Machine_ProbeFinished;
            _machine.PropertyChanged += _machine_PropertyChanged;
        }

        private void _machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(_machine.Mode))
            {
                StartProbingCommand.RaiseCanExecuteChanged();
                PauseProbingCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanStartProbing()
        {
            return _machine.Mode == OperatingMode.Manual && _currentHeightMap != null;
        }

        public bool CanPauseProbing()
        {
            return _machine.Mode == OperatingMode.ProbingHeightMap;
        }

        private void HeightMapProbeNextPoint()
        {
            if (_machine.Mode != OperatingMode.ProbingHeightMap)
                return;

            if (!_machine.Connected || _currentHeightMap == null || _currentHeightMap.NotProbed.Count == 0)
            {
                _machine.ProbeStop();
                return;
            }

            var nextPoint = _currentHeightMap.GetCoordinates(_currentHeightMap.NotProbed.Peek().Item1, _currentHeightMap.NotProbed.Peek().Item2);

            _machine.SendLine($"G0X{nextPoint.X.ToString("0.###", Constants.DecimalOutputFormat)}Y{nextPoint.Y.ToString("0.###", Constants.DecimalOutputFormat)}");

            _machine.SendLine($"G38.3Z-{_settings.ProbeMaxDepth.ToString("0.###", Constants.DecimalOutputFormat)}F{_settings.ProbeFeed.ToString("0.#", Constants.DecimalOutputFormat)}");

            _machine.SendLine("G91");
            _machine.SendLine($"G0Z{_settings.ProbeMinimumHeight.ToString("0.###", Constants.DecimalOutputFormat)}");
            _machine.SendLine("G90");
        }

        private void Machine_ProbeFinished(Vector3 position, bool success)
        {
            if (_machine.Mode != OperatingMode.ProbingHeightMap)
                return;

            if (!_machine.Connected || _currentHeightMap == null || _currentHeightMap.NotProbed.Count == 0)
            {
                _machine.ProbeStop();
                return;
            }

            if (!success && _settings.AbortOnProbeFail)
            {
                _machine.AddStatusMessage(StatusMessageTypes.FatalError, "Probe Failed! aborting");                
                _machine.ProbeStop();
                return;
            }

            Tuple<int, int> lastPoint = _currentHeightMap.NotProbed.Dequeue();

            _currentHeightMap.AddPoint(lastPoint.Item1, lastPoint.Item2, position.Z);

            if (_currentHeightMap.NotProbed.Count == 0)
            {
                _machine.SendLine($"G0Z{_settings.ProbeSafeHeight.ToString(Constants.DecimalOutputFormat)}");
                _machine.ProbeStop();
                return;
            }

            HeightMapProbeNextPoint();
        }

        public void StartProbing()
        {
            if (!_machine.Connected || _machine.Mode != OperatingMode.Manual || _currentHeightMap == null)
                return;

            if (_currentHeightMap.Progress == _currentHeightMap.TotalPoints)
                return;

            _machine.ProbeStart();

            if (_machine.Mode != OperatingMode.ProbingHeightMap)
                return;

            _machine.SendLine("G90");
            _machine.SendLine($"G0Z{_settings.ProbeSafeHeight.ToString("0.###", Constants.DecimalOutputFormat)}");

            HeightMapProbeNextPoint();
        }

        public void PauseProbing()
        {
            if (_machine.Mode != OperatingMode.ProbingHeightMap)
                return;

            _machine.ProbeStop();
        }

        public RelayCommand PauseProbingCommand { get; private set; }

        public RelayCommand StartProbingCommand { get; private set; }

        public HeightMap CurrentHeightMap
        {
            get { return _currentHeightMap; }
            set
            {
                Set(ref _currentHeightMap, value);
                StartProbingCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
