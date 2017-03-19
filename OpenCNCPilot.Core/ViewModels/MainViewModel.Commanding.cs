using LagoVista.Core.Commanding;
using LagoVista.Core.GCode;
using System;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MainViewModel
    {
        private void InitCommands()
        {
            OpenHeightMapCommand = new RelayCommand(OpenHeightMapFile, CanPerformFileOperation);
            OpenGCodeFileCommand = new RelayCommand(OpenGCodeFile, CanPerformFileOperation);
            CloseFileCommand = new RelayCommand(CloseFile, CanPerformFileOperation);
            ClearHeightMapCommand = new RelayCommand(ClearHeightMap, CanClearHeightMap);
            ArcToLineCommand = new RelayCommand(ArcToLine, CanConvertArcToLine);
            ApplyHeightMapCommand = new RelayCommand(ApplyHeightMap, CanApplyHeightMap);

            StartProbeHeightMapCommand = new RelayCommand(ProbeHeightMap);

            ShowCutoutMillingGCodeCommand = new RelayCommand(GenerateMillingGCode, CanGenerateGCode);
            ShowDrillGCodeCommand = new RelayCommand(GenerateDrillGCode, CanGenerateGCode);
            ShowHoldDownGCodeCommand = new RelayCommand(GenerateHoldDownGCode, CanGenerateGCode);

            ShowTopEtchingGCodeCommand = new RelayCommand(ShowTopEtchingGCode, CanGenerateTopEtchingGCode);
            ShowBottomEtchingGCodeCommand = new RelayCommand(ShowBottomEtchingGCode, CanGenerateBottomEtchingGCode);

            SetMetricUnitsCommand = new RelayCommand(SetMetricUnits, CanChangeUnits);
            SetImperialUnitsCommand = new RelayCommand(SetImperialUnits, CanChangeUnits);

            OpenEagleBoardFileCommand = new RelayCommand(OpenEagleBoardFile);
            CloseEagleBoardFileCommand = new RelayCommand(CloseEagleBoardFile);

            SetAbsolutePositionModeCommand = new RelayCommand(SetAbsolutePositionMode, CanSetPositionMode);
            SetIncrementalPositionModeCommand = new RelayCommand(SetIncrementalPositionMode, CanSetPositionMode);

            Machine.PropertyChanged += _machine_PropertyChanged;
            Machine.PCBManager.PropertyChanged += PCBManager_PropertyChanged;
        }

        private void PCBManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.PCBManager.HasBoard))
            {
                DispatcherServices.Invoke(() =>
                {
                    ShowHoldDownGCodeCommand.RaiseCanExecuteChanged();
                    ShowDrillGCodeCommand.RaiseCanExecuteChanged();
                    ShowCutoutMillingGCodeCommand.RaiseCanExecuteChanged();
                });
            }
        }

        private void _machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.Mode))
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

            if (e.PropertyName == nameof(Machine.GCodeFileManager.HasValidFile))
            {
                ArcToLineCommand.RaiseCanExecuteChanged();
                ApplyHeightMapCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanGenerateGCode()
        {
            return Machine.PCBManager.HasBoard;
        }

        public bool CanGenerateTopEtchingGCode()
        {
            return true;
        }

        public bool CanGenerateBottomEtchingGCode()
        {

            return true;
        }

        public bool CanSetPositionMode()
        {
            return Machine.Mode == OperatingMode.Manual && Machine.Connected;
        }

        public bool CanConvertArcToLine()
        {
            return Machine.GCodeFileManager.HasValidFile;
        }

        public bool CanChangeUnits()
        {
            return Machine.Mode == OperatingMode.Manual && Machine.Connected;
        }

        public bool CanApplyHeightMap()
        {
            return true;
            /*return Machine.GCodeFileManager.HasValidFile &&
                   Machine.HeightMapManager.HasHeightMap &&
                   Machine.HeightMapManager.HeightMap.Status == Models.HeightMapStatus.Populated;*/
        }

        private bool CanPerformFileOperation(Object instance)
        {
            return (Machine.Mode != OperatingMode.SendingGCodeFile);
        }

        public bool CanClearHeightMap()
        {
            return Machine.HeightMapManager.HeightMap != null;
        }

        public RelayCommand OpenEagleBoardFileCommand { get; private set; }
        public RelayCommand CloseEagleBoardFileCommand { get; private set; }

        public RelayCommand SetImperialUnitsCommand { get; private set; }
        public RelayCommand SetMetricUnitsCommand { get; private set; }

        public RelayCommand SetAbsolutePositionModeCommand { get; private set; }
        public RelayCommand SetIncrementalPositionModeCommand { get; private set; }

        public RelayCommand OpenHeightMapCommand { get; private set; }
        public RelayCommand OpenGCodeFileCommand { get; private set; }
        public RelayCommand CloseFileCommand { get; private set; }

        public RelayCommand ClearHeightMapCommand { get; private set; }

        public RelayCommand ApplyHeightMapCommand { get; private set; }
        public RelayCommand ArcToLineCommand { get; private set; }

        public RelayCommand ShowTopEtchingGCodeCommand { get; private set; }
        public RelayCommand ShowBottomEtchingGCodeCommand { get; private set; }

        public RelayCommand ShowCutoutMillingGCodeCommand { get; private set; }
        public RelayCommand ShowDrillGCodeCommand { get; private set; }
        public RelayCommand ShowHoldDownGCodeCommand { get; private set; }
        public RelayCommand StartProbeHeightMapCommand { get; private set; }

    }
}
