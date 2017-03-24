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
            OpenGCodeCommand = new RelayCommand(OpenGCodeFile, CanPerformFileOperation);
            ClearGCodeCommand = new RelayCommand(CloseFile, CanPerformFileOperation);
            ArcToLineCommand = new RelayCommand(ArcToLine, CanConvertArcToLine);

            SaveHeightMapCommand = new RelayCommand(SaveHeightMap, CanSaveHeightMap);

            ApplyHeightMapCommand = new RelayCommand(ApplyHeightMap, CanApplyHeightMap);
            ClearHeightMapCommand = new RelayCommand(ClearHeightMap, CanClearHeightMap);

            SaveModifiedGCodeCommamnd = new RelayCommand(SaveModifiedGCode, CanSaveModifiedGCode);

            StartProbeHeightMapCommand = new RelayCommand(ProbeHeightMap);

            ShowCutoutMillingGCodeCommand = new RelayCommand(GenerateMillingGCode, CanGenerateGCode);
            ShowDrillGCodeCommand = new RelayCommand(GenerateDrillGCode, CanGenerateGCode);
            ShowHoldDownGCodeCommand = new RelayCommand(GenerateHoldDownGCode, CanGenerateGCode);

            ShowTopEtchingGCodeCommand = new RelayCommand(ShowTopEtchingGCode, CanGenerateTopEtchingGCode);
            ShowBottomEtchingGCodeCommand = new RelayCommand(ShowBottomEtchingGCode, CanGenerateBottomEtchingGCode);

            SetMetricUnitsCommand = new RelayCommand(SetMetricUnits, CanChangeUnits);
            SetImperialUnitsCommand = new RelayCommand(SetImperialUnits, CanChangeUnits);

            OpenEagleBoardFileCommand = new RelayCommand(OpenEagleBoardFile, CanOpenEagleBoard);
            CloseEagleBoardFileCommand = new RelayCommand(CloseEagleBoardFile, CanCloseEagleBoard);

            SetAbsolutePositionModeCommand = new RelayCommand(SetAbsolutePositionMode, CanSetPositionMode);
            SetIncrementalPositionModeCommand = new RelayCommand(SetIncrementalPositionMode, CanSetPositionMode);

            Machine.PropertyChanged += _machine_PropertyChanged;
            Machine.PCBManager.PropertyChanged += PCBManager_PropertyChanged;
            Machine.HeightMapManager.PropertyChanged += HeightMapManager_PropertyChanged;
        }

        private void HeightMapManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.HeightMapManager.HasHeightMap) ||
               e.PropertyName == nameof(Machine.HeightMapManager.Status) ||
               e.PropertyName == nameof(Machine.HeightMapManager.HeightMapDirty))
            {
                DispatcherServices.Invoke(() =>
                {
                    ApplyHeightMapCommand.RaiseCanExecuteChanged();
                    SaveHeightMapCommand.RaiseCanExecuteChanged();
                    StartProbeHeightMapCommand.RaiseCanExecuteChanged();
                    ClearHeightMapCommand.RaiseCanExecuteChanged();
                });
            }
        }

        private void PCBManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.PCBManager.HasBoard) ||
                e.PropertyName == nameof(Machine.PCBManager.HasProject) ||
                e.PropertyName == nameof(Machine.PCBManager.HasTopEtching) ||
                e.PropertyName == nameof(Machine.PCBManager.HasBottomEtching))
            {
                DispatcherServices.Invoke(() =>
                {
                    ShowHoldDownGCodeCommand.RaiseCanExecuteChanged();
                    ShowDrillGCodeCommand.RaiseCanExecuteChanged();
                    ShowCutoutMillingGCodeCommand.RaiseCanExecuteChanged();
                    ShowTopEtchingGCodeCommand.RaiseCanExecuteChanged();
                    ShowBottomEtchingGCodeCommand.RaiseCanExecuteChanged();
                });
            }
        }

        private void _machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.Mode))
            {
                DispatcherServices.Invoke(() =>
                {
                    OpenEagleBoardFileCommand.RaiseCanExecuteChanged();
                    CloseEagleBoardFileCommand.RaiseCanExecuteChanged();
                    OpenHeightMapCommand.RaiseCanExecuteChanged();
                    OpenGCodeCommand.RaiseCanExecuteChanged();
                    ClearGCodeCommand.RaiseCanExecuteChanged();
                    SetMetricUnitsCommand.RaiseCanExecuteChanged();
                    SetImperialUnitsCommand.RaiseCanExecuteChanged();
                    ShowBottomEtchingGCodeCommand.RaiseCanExecuteChanged();
                    ShowTopEtchingGCodeCommand.RaiseCanExecuteChanged();
                    ApplyHeightMapCommand.RaiseCanExecuteChanged();
                });
            }

            if (e.PropertyName == nameof(Machine.GCodeFileManager.HasValidFile))
            {
                ArcToLineCommand.RaiseCanExecuteChanged();
                ApplyHeightMapCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanOpenEagleBoard()
        {
            return (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected); 
        }

        public bool CanCloseEagleBoard()
        {
            return Machine.PCBManager.HasBoard && !Machine.PCBManager.HasProject && (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected);
        }

        public bool CanGenerateGCode()
        {
            return Machine.PCBManager.HasBoard && (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected);
        }

        public bool CanGenerateTopEtchingGCode()
        {
            return Machine.PCBManager.HasTopEtching && (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected);
        }

        public bool CanGenerateBottomEtchingGCode()
        {

            return Machine.PCBManager.HasBottomEtching && (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected);
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
            return Machine.GCodeFileManager.HasValidFile &&
                   Machine.HeightMapManager.HasHeightMap &&
                  Machine.HeightMapManager.HeightMap.Status == Models.HeightMapStatus.Populated;
        }

        public bool CanSaveModifiedGCode()
        {
            return Machine.GCodeFileManager.HasValidFile &&
                   Machine.GCodeFileManager.IsDirty;
        }

        public bool CanSaveHeightMap()
        {
            return Machine.HeightMapManager.HasHeightMap &&
                  Machine.HeightMapManager.HeightMap.Status == Models.HeightMapStatus.Populated;
        }

        private bool CanPerformFileOperation(Object instance)
        {
            return (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected);
        }

        public bool CanClearHeightMap()
        {
            return Machine.HeightMapManager.HasHeightMap &&
                  Machine.HeightMapManager.HeightMap.Status == Models.HeightMapStatus.Populated;
        }

        public RelayCommand ArcToLineCommand { get; private set; }

        public RelayCommand OpenEagleBoardFileCommand { get; private set; }
        public RelayCommand CloseEagleBoardFileCommand { get; private set; }

        public RelayCommand SetImperialUnitsCommand { get; private set; }
        public RelayCommand SetMetricUnitsCommand { get; private set; }

        public RelayCommand SetAbsolutePositionModeCommand { get; private set; }
        public RelayCommand SetIncrementalPositionModeCommand { get; private set; }

        public RelayCommand OpenHeightMapCommand { get; private set; }
        public RelayCommand SaveHeightMapCommand { get; private set; }
        public RelayCommand ApplyHeightMapCommand { get; private set; }
        public RelayCommand ClearHeightMapCommand { get; private set; }
        public RelayCommand StartProbeHeightMapCommand { get; private set; }

        public RelayCommand OpenGCodeCommand { get; private set; }
        public RelayCommand SaveModifiedGCodeCommamnd { get; private set; }
        public RelayCommand ClearGCodeCommand { get; private set; }

        public RelayCommand ShowTopEtchingGCodeCommand { get; private set; }
        public RelayCommand ShowBottomEtchingGCodeCommand { get; private set; }
        public RelayCommand ShowCutoutMillingGCodeCommand { get; private set; }
        public RelayCommand ShowDrillGCodeCommand { get; private set; }
        public RelayCommand ShowHoldDownGCodeCommand { get; private set; }
    }
}
