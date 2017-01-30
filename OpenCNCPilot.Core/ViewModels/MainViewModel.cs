using LagoVista.Core.Commanding;
using LagoVista.Core.GCode;
using LagoVista.Core.ViewModels;
using LagoVista.GCode.Sender.Models;
using System;

namespace LagoVista.GCode.Sender.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        IMachine _machine;
        Settings _settings;

        HeightMapViewModel _heightMapVieModel;

        public MainViewModel(IMachine machine, Settings settings)
        {
            _machine = machine;
            _settings = settings;

            OpenHeightMapCommand = new RelayCommand(OpenHeightMapFile, CanPerformFileOperation);
            OpenGCodeFileCommand = new RelayCommand(OpenGCodeFile, CanPerformFileOperation);
            CloseFileCommand = new RelayCommand(CloseFile, CanPerformFileOperation);
            ClearHeightMapCommand = new RelayCommand(ClearHeightMap, CanClearHeightMap);
            ArcToLineCommand = new RelayCommand(ArcToLine, CurrentlyJob);
            ApplyHeightMapCommand = new RelayCommand(ApplyHeightMap, CanApplyHeightMap);

            SetMetricUnitsCommand = new RelayCommand(SetMetricUnits, CanSetMetricUnits);
            SetImperialUnitsCommand = new RelayCommand(SetImperialUnits, CanSetImperialUnits);

            _machine.PropertyChanged += _machine_PropertyChanged;

            HeightMapVM = new HeightMapViewModel(_machine, _settings);
        }

        private void _machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_machine.Mode))
            {
                DispatcherServices.Invoke(() =>
                {
                    OpenHeightMapCommand.RaiseCanExecuteChanged();
                    OpenGCodeFileCommand.RaiseCanExecuteChanged();
                    CloseFileCommand.RaiseCanExecuteChanged();
                    SetMetricUnitsCommand.RaiseCanExecuteChanged();
                    SetImperialUnitsCommand.RaiseCanExecuteChanged();
                });
            }

            if (e.PropertyName == nameof(_machine.HasJob))
            {
                ArcToLineCommand.RaiseCanExecuteChanged();
                ApplyHeightMapCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanClearHeightMap()
        {
            return HeightMapVM.CurrentHeightMap != null;
        }


        public bool CurrentlyJob()
        {
            return _machine.HasJob;
        }

        public bool CanApplyHeightMap()
        {
            return _machine.HasJob && HeightMapVM.CurrentHeightMap != null;
        }

        private bool CanPerformFileOperation(Object instance)
        {
            return (_machine.Mode != OperatingMode.SendingJob);
        }

        public void ApplyHeightMap()
        {
            if (_machine.HasJob && HeightMapVM.CurrentHeightMap != null)
            {
                _machine.CurrentJob.ApplyHeightMap(HeightMapVM.CurrentHeightMap);
            }
        }

        public void Togglek()
        {
            if (_machine.Mode != OperatingMode.Manual)
                return;

            if (_machine.DistanceMode == ParseDistanceMode.Absolute)
                _machine.SendLine("G91");
            else
                _machine.SendLine("G90");
        }

        private void ButtonArcPlane_Click()
        {
            if (_machine.Mode != OperatingMode.Manual)
                return;

            if (_machine.Plane != ArcPlane.XY)
                _machine.SendLine("G17");
        }


        //http://www.cnccookbook.com/CCCNCGCodeG20G21MetricImperialUnitConversion.htm
        public bool CanSetImperialUnits()
        {
            return _machine.Mode == OperatingMode.Manual && _machine.Unit == ParseUnit.Metric;
        }

        public void SetImperialUnits()
        {
            if (_machine.Mode != OperatingMode.Manual)
                return;

            if (_machine.Unit == ParseUnit.Metric)
            {
                _machine.SendLine("G20");
            }
        }

        public bool CanSetMetricUnits()
        {
            return _machine.Mode == OperatingMode.Manual && _machine.Unit == ParseUnit.Imperial;
        }

        public void SetMetricUnits()
        {
            if (_machine.Mode != OperatingMode.Manual)
                return;

            if (_machine.Unit == ParseUnit.Imperial)
            {
                _machine.SendLine("G20");
            }
        }


        public void ClearHeightMap()
        {
            HeightMapVM.CurrentHeightMap = null;
        }

        public async void ArcToLine()
        {
            if (_machine.HasJob)
            {
                var result = await Popups.PromptForDoubleAsync("Convert Line to Arch", _settings.ArcToLineSegmentLength, "Enter Arc Width", true);
                if (result.HasValue)
                    _machine.CurrentJob.ArcToLines(result.Value);
            }
        }


        public async void OpenGCodeFile(object instance)
        {
            var file = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!String.IsNullOrEmpty(file))
            {
                _machine.SetFile(GCodeFile.Load(file));
            }
        }

        public async void OpenHeightMapFile(object instance)
        {
            var heightMap = await HeightMap.OpenAsync(_settings);
            if (heightMap != null)
            {
                _heightMapVieModel.CurrentHeightMap = heightMap;
            }
        }

        public void CloseFile(object instance)
        {
            _machine.ClearFile();
        }


        public HeightMapViewModel HeightMapVM
        {
            get { return _heightMapVieModel; }
            set { Set(ref _heightMapVieModel, value); }
        }

        public IMachine Machine
        {
            get { return _machine; }
        }


        public RelayCommand SetImperialUnitsCommand { get; private set; }
        public RelayCommand SetMetricUnitsCommand { get; private set; }


        public RelayCommand OpenHeightMapCommand { get; private set; }
        public RelayCommand OpenGCodeFileCommand { get; private set; }
        public RelayCommand CloseFileCommand { get; private set; }

        public RelayCommand ClearHeightMapCommand { get; private set; }

        public RelayCommand ApplyHeightMapCommand { get; private set; }
        public RelayCommand ArcToLineCommand { get; private set; }

    }
}
