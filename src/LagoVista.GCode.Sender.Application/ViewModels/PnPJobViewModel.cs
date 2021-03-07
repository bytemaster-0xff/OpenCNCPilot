using LagoVista.Core.Commanding;
using LagoVista.Core.GCode;
using LagoVista.Core.Models.Drawing;
using LagoVista.EaglePCB.Models;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.PickAndPlace.Managers;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.Repos;
using LagoVista.PickAndPlace.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public class PnPJobViewModel : MachineVisionViewModelBase
    {
        private bool _isEditing;
        private bool _isPaused;
        private bool _isDirty = false;
        private BOM _billOfMaterials;
        private FeederLibrary _feederLibrary;

        private int _partIndex = 0;

        private enum MVLocatorState
        {
            Idle,
            MachineFidicual,
            BoardFidicual1,
            BoardFidicual2,
            Default,
            NozzleCalibration,
        }

        MVLocatorState _mvLocatorState = MVLocatorState.Default;

        public PnPJobViewModel(IMachine machine, PnPJob job) : base(machine)
        {
            _billOfMaterials = new BOM(job.Board);
            _job = job;
            _isDirty = true;

            SaveCommand = new RelayCommand(() => SaveJob());
            CloseCommand = new RelayCommand(Close);
            CloneCommand = new RelayCommand(CloneConfiguration);

            PeformMachineAlignmentCommand = new RelayCommand(PerformMachineAlignment);

            GoToPartOnBoardCommand = new RelayCommand(GoToPartOnBoard);
            GoToPartPositionInTrayCommand = new RelayCommand(GoToPartPositionInTray);

            SelectMachineFileCommand = new RelayCommand(SelectMachineFile);

            HomingCycleCommand = new RelayCommand(() => Machine.HomingCycle());

            AlignBottomCameraCommand = new RelayCommand(() => AlignBottomCamera());

            ResetCurrentComponentCommand = new RelayCommand(ResetCurrentComponent, () => SelectedPartRow != null);

            GoToWorkHomeCommand = new RelayCommand(() => GotoWorkspaceHome());
            SetWorkHomeCommand = new RelayCommand(() => Machine.SetWorkspaceHome());

            MoveToPreviousComponentInTapeCommand = new RelayCommand(MoveToPreviousComponent, () => SelectedPartRow != null && SelectedPartRow.CurrentPartIndex > 0);
            MoveToNextComponentInTapeCommand = new RelayCommand(MoveToNextComponentInTape, () => SelectedPartRow != null && SelectedPartRow.CurrentPartIndex < SelectedPartRow.PartCount);
            RefreshConfigurationPartsCommand = new RelayCommand(PopulateConfigurationParts);
            GoToPartInTrayCommand = new RelayCommand(GoToPartPositionInTray);

            PlaceCurrentPartCommand = new RelayCommand(PlacePart, CanPlacePart);
            PlaceAllPartsCommand = new RelayCommand(PlaceAllParts, CanPlacePart);
            PausePlacmentCommand = new RelayCommand(PausePlacement, CanPausePlacement);

            SetFiducialCalibrationCommand = new RelayCommand((prm) => SetFiducialCalibration(prm));

            CalibrateBottomCameraCommand = new RelayCommand(() => CalibrateBottomCamera());

            _feederLibrary = new FeederLibrary();

            BuildFlavors = job.BuildFlavors;
            SelectedBuildFlavor = job.BuildFlavors.FirstOrDefault();
            if (SelectedBuildFlavor == null)
            {
                SelectedBuildFlavor = new BuildFlavor()
                {
                    Name = "Default"
                };

                foreach (var entry in _billOfMaterials.SMDEntries)
                {
                    foreach (var component in entry.Components)
                    {
                        component.Included = true;
                        SelectedBuildFlavor.Components.Add(component);
                    }
                }

                job.BuildFlavors.Add(SelectedBuildFlavor);
            }

            PartPackManagerVM = new PartPackManagerViewModel(Machine, this);
            PackageLibraryVM = new PackageLibraryViewModel();

            GoToFiducial1Command = new RelayCommand(() => GoToFiducial(1));
            GoToFiducial2Command = new RelayCommand(() => GoToFiducial(2));

            PopulateParts();
            PopulateConfigurationParts();
        }

        private void PopulateParts()
        {
            Parts.Clear();

            foreach (var entry in _billOfMaterials.SMDEntries)
            {
                if (!Parts.Where(prt => prt.PackageName == entry.Package.Name &&
                                        prt.LibraryName == entry.Package.LibraryName &&
                                        prt.Value == entry.Value).Any())
                {
                    Parts.Add(new Part()
                    {
                        Count = entry.Components.Count,
                        LibraryName = entry.Package.LibraryName,
                        PackageName = entry.Package.Name,
                        Value = entry.Value
                    });
                }
            }
        }

        private void SetFiducialCalibration(object obj)
        {
            var idx = Convert.ToInt32(obj);
            switch (idx)
            {
                case 1:
                    {
                        var gcode = $"G92 X{Job.BoardFiducial1.X} Y{Job.BoardFiducial1.Y}";
                        Machine.SendCommand(gcode);
                    }
                    break;
                case 2:
                    {
                        var gcode = $"G92 X{Job.BoardFiducial2.X} Y{Job.BoardFiducial2.Y}";
                        Machine.SendCommand(gcode);
                    }
                    break;
            }
        }

        private async void GoToFiducial(int idx)
        {
            Machine.SendCommand(SafeHeightGCodeGCode());

            await Machine.SetViewTypeAsync(ViewTypes.Camera);

            ShowTopCamera = true;

            switch (idx)
            {
                case 0:
                    {
                        var gcode = $"G1 X{Machine.Settings.MachineFiducial.X} Y{Machine.Settings.MachineFiducial.Y} F{Machine.Settings.FastFeedRate}";
                        Machine.SendCommand(gcode);
                    }
                    break;
                case 1:
                    {
                        var gcode = $"G1 X{Job.BoardFiducial1.X} Y{Job.BoardFiducial1.Y} F{Machine.Settings.FastFeedRate}";
                        Machine.SendCommand(gcode);
                    }
                    break;
                case 2:
                    {
                        var gcode = $"G1 X{Job.BoardFiducial2.X} Y{Job.BoardFiducial2.Y} F{Machine.Settings.FastFeedRate}";
                        Machine.SendCommand(gcode);
                    }
                    break;
            }
        }

        public void CalibrateBottomCamera()
        {
            AlignBottomCamera();
            _targetAngle = 0;
            _mvLocatorState = MVLocatorState.NozzleCalibration;
            SelectMVProfile("nozzlecalibration");
            Machine.SendCommand($"G0 E0");
            _averagePoints = new List<Point2D<double>>();

        }

        public bool CanPlacePart()
        {
            if (_selectedPart == null)
                return false;

            if (_isPlacingParts)
                return false;

            return true;
        }

        public void PausePlacement(Object obj)
        {
            _isPlacingParts = false;
            _isPaused = true;
        }

        public bool CanPausePlacement(Object obj)
        {
            return _isPlacingParts;
        }

        public void GotoWorkspaceHome()
        {
            Machine.SendCommand(SafeHeightGCodeGCode());
            Machine.GotoWorkspaceHome();
            ShowBottomCamera = false;
            ShowTopCamera = true;
        }

        public void PerformMachineAlignment()
        {
            Machine.SendCommand(SafeHeightGCodeGCode());
            _mvLocatorState = MVLocatorState.Idle;

            Machine.ViewType = ViewTypes.Camera;
            Machine.TopLightOn = true;

            Machine.GotoWorkspaceHome();

            SelectMVProfile("brdfiducual");

            Machine.SendCommand(DwellGCode(250));

            GoToFiducial(0);

            ShowCircles = true;

            _mvLocatorState = MVLocatorState.MachineFidicual;
        }

        public async void Close()
        {
            await SaveJob();
            CloseScreen();
        }

        private void PopulateConfigurationParts()
        {
            ConfigurationParts.Clear();
            var commonParts = SelectedBuildFlavor.Components.Where(prt => prt.Included).GroupBy(prt => prt.Key);

            foreach (var entry in commonParts)
            {
                var part = new PlaceableParts()
                {
                    Count = entry.Count(),
                    Value = entry.First().Value.ToUpper(),
                    Package = entry.First().PackageName.ToUpper(),

                };

                part.Parts = new ObservableCollection<Component>();

                foreach (var specificPart in entry)
                {
                    var placedPart = SelectedBuildFlavor.Components.Where(cmp => cmp.Name == specificPart.Name && cmp.Key == specificPart.Key).FirstOrDefault();
                    if (placedPart != null)
                    {
                        part.Parts.Add(placedPart);
                    }
                }

                ConfigurationParts.Add(part);
            }

            if (_pnpMachine != null)
            {
                foreach (var part in ConfigurationParts)
                {
                    PnPMachineManager.ResolvePart(_pnpMachine, part);
                }
            }
        }

        private Dictionary<int, Point2D<double>> _nozzleCalibration;
        int samplesAtPoint = 0;
        int _targetAngle = 0;

        private List<Point2D<double>> _averagePoints;

        public override void CircleLocated(Point2D<double> point, double diameter, Point2D<double> stdDeviation)
        {
            switch (_mvLocatorState)
            {
                case MVLocatorState.MachineFidicual:
                    JogToLocation(point);
                    break;

                case MVLocatorState.BoardFidicual1:
                    JogToLocation(point);
                    break;
                case MVLocatorState.BoardFidicual2:
                    JogToLocation(point);
                    break;
                case MVLocatorState.NozzleCalibration:
                    Debug.WriteLine($"Found Circle: {Machine.MachinePosition.X},{Machine.MachinePosition.Y} - {stdDeviation.X},{stdDeviation.Y}");

                    if (_targetAngle == Convert.ToInt32(Machine.Tool2))
                    {
                        samplesAtPoint++;

                        if (samplesAtPoint > 50)
                        {
                            var avgX = _averagePoints.Average(pt => pt.X);
                            var avgY = _averagePoints.Average(pt => pt.Y);
                            _nozzleCalibration.Add(Convert.ToInt32(Machine.Tool2), new Point2D<double>(avgX, avgY));
                            _targetAngle = Convert.ToInt32(Machine.Tool2 + 15.0);
                            Machine.SendCommand($"G0 E{_targetAngle}");
                            _averagePoints.Clear();
                            samplesAtPoint = 0;
                        }
                        else
                        {
                            _averagePoints.Add(new Point2D<double>(point.X, point.Y));
                        }

                        if (Machine.Tool2 >= 360)
                        {
                            _mvLocatorState = MVLocatorState.Idle;
                            foreach (var key in _nozzleCalibration.Keys)
                            {
                                Debug.WriteLine($"{key},{_nozzleCalibration[key].X},{_nozzleCalibration[key].Y}");
                            }

                            var maxX = _nozzleCalibration.Values.Max(ca => ca.X);
                            var maxY = _nozzleCalibration.Values.Max(ca => ca.Y);

                            var minX = _nozzleCalibration.Values.Min(ca => ca.X);
                            var minY = _nozzleCalibration.Values.Min(ca => ca.Y);

                            var preCalX = Machine.MachinePosition.X;
                            var preCalY = Machine.MachinePosition.Y;


                            var top = _nozzleCalibration.First(pt => pt.Value.Y == maxY);
                            
                            var topAngle = top.Key;
                            var offsetX = top.Value.X / 20.0;
                            var offsetY = top.Value.Y / 20.0;

                            //var offsetX = ((maxX - minX) / 60.0);
                            //var offsetY = ((maxY - minY) / 60.0);

                            Machine.SendCommand("G91");
                            Machine.SendCommand($"G0 X-{offsetX} Y{+offsetY}");
                            Machine.SendCommand("G90");
                            

                            Debug.WriteLine($"MIN: {minX},{minY} MAX: {maxX},{maxY}, Adjusting to offset: {offsetX},{offsetY} - Top Angle: {topAngle}");

                            Machine.SendCommand($"G0 E{topAngle}");
                            Machine.SendCommand($"G92 E0 X{preCalX} Y{preCalY}");
                        }
                    }

                    break;
                default:
                    if (PartPackManagerVM.IsLocating)
                    {
                        JogToLocation(point);
                    }
                    break;
            }
        }

        private void SetNewHome()
        {
            Machine.SendCommand($"G92 X{Machine.Settings.MachineFiducial.X} Y{Machine.Settings.MachineFiducial.Y}");
            Machine.SendCommand(SafeHeightGCodeGCode());
            var gcode = $"G1 X0 Y0 F{Machine.Settings.FastFeedRate}";
            Machine.SendCommand(gcode);

            ShowCircles = false;

            _mvLocatorState = MVLocatorState.Default;
        }

        public override void CircleCentered(Point2D<double> point, double diameter)
        {
            switch (_mvLocatorState)
            {
                case MVLocatorState.MachineFidicual:
                    SetNewHome();
                    break;
                default:
                    if (PartPackManagerVM.IsLocating)
                    {
                        PartPackManagerVM.FoundLocation(point, diameter);
                    }
                    break;
            }
        }

        public async void AlignBottomCamera()
        {
            if (Machine.Settings.PartInspectionCamera?.AbsolutePosition != null)
            {
                _nozzleCalibration = new Dictionary<int, Point2D<double>>();
                Machine.SendCommand(SafeHeightGCodeGCode());

                await Machine.SetViewTypeAsync(ViewTypes.Tool1);

                Machine.TopLightOn = false;
                Machine.BottomLightOn = true;
                SelectMVProfile("nozzle");
                ShowCircles = true;

                ShowBottomCamera = true;

                Machine.SendCommand($"G0 X{Machine.Settings.PartInspectionCamera.AbsolutePosition.X} Y{Machine.Settings.PartInspectionCamera.AbsolutePosition.Y} Z{Machine.Settings.PartInspectionCamera.FocusHeight} F1{Machine.Settings.FastFeedRate}");
            }
        }

        public override async Task InitAsync()
        {
            await base.InitAsync();
            FeederTypes = await _feederLibrary.GetFeedersAsync();
            LoadingMask = false;

            Machine.TopLightOn = true;
            Machine.BottomLightOn = false;

            if (!String.IsNullOrEmpty(_job.PnPMachinePath) && System.IO.File.Exists(_job.PnPMachinePath))
            {
                PnPMachine = await PnPMachineManager.GetPnPMachineAsync(_job.PnPMachinePath);
                PartPackManagerVM.SetMachine(PnPMachine);
                PackageLibraryVM.SetMachine(PnPMachine);
            }

            StartCapture();
        }

        public async void CloneConfiguration()
        {
            var clonedName = await Popups.PromptForStringAsync("Cloned configuration name", isRequired: true);
            var clonedFlavor = SelectedBuildFlavor.Clone(clonedName);
            BuildFlavors.Add(clonedFlavor);
            SelectedBuildFlavor = clonedFlavor;
        }

        public async void ResetCurrentComponent()
        {
            SelectedPartRow.CurrentPartIndex = 0;
            RaisePropertyChanged(nameof(XPartInTray));
            RaisePropertyChanged(nameof(RotationInTape));
            RaisePropertyChanged(nameof(YPartInTray));
            RaisePropertyChanged(nameof(SelectedPartRow));
            GoToPartPositionInTray();
            await SaveJob();
        }

        public async void MoveToPreviousComponent()
        {
            if (SelectedPartRow.CurrentPartIndex > 0)
            {
                SelectedPartRow.CurrentPartIndex--;
                MoveToNextComponentInTapeCommand.RaiseCanExecuteChanged();
                MoveToPreviousComponentInTapeCommand.RaiseCanExecuteChanged();
                ResetCurrentComponentCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(XPartInTray));
                RaisePropertyChanged(nameof(RotationInTape));
                RaisePropertyChanged(nameof(YPartInTray));
                RaisePropertyChanged(nameof(SelectedPartRow));
                await SaveJob();
                GoToPartPositionInTray();
            }
        }

        public async void MoveToNextComponentInTape()
        {
            if (SelectedPartRow.CurrentPartIndex < SelectedPartRow.PartCount)
            {
                SelectedPartRow.CurrentPartIndex++;
                MoveToNextComponentInTapeCommand.RaiseCanExecuteChanged();
                MoveToPreviousComponentInTapeCommand.RaiseCanExecuteChanged();
                ResetCurrentComponentCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(XPartInTray));
                RaisePropertyChanged(nameof(RotationInTape));
                RaisePropertyChanged(nameof(YPartInTray));
                RaisePropertyChanged(nameof(SelectedPartRow));
                await SaveJob();
                GoToPartPositionInTray();
            }
        }

        async Task SendInstructionSequenceAsync(List<string> cmds)
        {
            var file = GCodeFile.FromList(cmds, Logger);
            Machine.SetFile(file);
            Machine.GCodeFileManager.StartJob();
            while (Machine.Mode == OperatingMode.SendingGCodeFile) await Task.Delay(1);
        }

        public string SafeHeightGCodeGCode()
        {
            return $"G0 Z{Machine.Settings.ToolSafeMoveHeight} F{Machine.Settings.FastFeedRate}";
        }

        public string PickHeightGCode()
        {
            return $"G0 Z{Machine.Settings.ToolPickHeight} F{Machine.Settings.FastFeedRate}";
        }

        public string PlaceHeightGCode(PickAndPlace.Models.Package package)
        {
            return $"G0 Z{Machine.Settings.ToolBoardHeight - package.Height} F{Machine.Settings.FastFeedRate}";
        }

        public string DwellGCode(int pauseMS)
        {
            return $"G4 P{pauseMS}";
        }

        public string RotationGCode(double cRotation)
        {
            if (cRotation == 360)
                cRotation = 0;
            else if (cRotation > 360)
                cRotation -= 360;
            else if (cRotation == 270)
                cRotation = -90;

            var scaledAngle = cRotation;
            return $"G0 E{scaledAngle} F5000";
        }

        public string ProduceVacuumGCode(bool value)
        {
            switch (Machine.Settings.MachineType)
            {
                case FirmwareTypes.Repeteir_PnP: return $"M42 P29 S{(value ? 255 : 0)}";
                case FirmwareTypes.LagoVista_PnP: return $"M64 S{(value ? 255 : 0)}";
            }

            throw new Exception($"Can't produce vacuum GCode for machien type: {Machine.Settings.MachineType} .");
        }

        public string WaitForComplete()
        {
            return "M400";
        }

        public async void PlaceAllParts()
        {
            //_partIndex = 0;
            Machine.LocationUpdateEnabled = false;

            // Make sure any pending location requests have completed.
            await Task.Delay(1000);
            while (SelectedPart != null && !_isPaused)
            {
                await PlacePartAsync(true);
            }

            Machine.LocationUpdateEnabled = true;

            Machine.Vacuum1On = false;
            Machine.Vacuum2On = false;
        }

        public async void PlacePart()
        {
            Machine.LocationUpdateEnabled = false;

            // Make sure any pending location requests have completed.
            await Task.Delay(1000);
            await PlacePartAsync();
            Machine.LocationUpdateEnabled = true;
        }

        public async Task PlacePartAsync(bool multiple = false)
        {
            if (_partIndex < SelectedPart.Parts.Count)
            {
                _isPlacingParts = true;
                PlaceCurrentPartCommand.RaiseCanExecuteChanged();
                PlaceAllPartsCommand.RaiseCanExecuteChanged();
                PausePlacmentCommand.RaiseCanExecuteChanged();

                if (!Machine.Vacuum1On || !Machine.Vacuum2On)
                {
                    Machine.Vacuum1On = true;
                    Machine.Vacuum2On = true;
                    await Task.Delay(1000);
                }

                await Machine.SetViewTypeAsync(ViewTypes.Tool1);

                if (_selectPartToBePlaced == null)
                {
                    _partIndex = 0;
                    _selectPartToBePlaced = SelectedPart.Parts[_partIndex];
                    RaisePropertyChanged(nameof(SelectedPartToBePlaced));
                }

                var cmds = new List<string>();

                cmds.Add(SafeHeightGCodeGCode()); // Move to move height
                cmds.Add(GetGoToPartInTrayGCode());
                cmds.Add(RotationGCode(0)); // Ensure we are at zero position before picking up part.
                cmds.Add(WaitForComplete());
                cmds.Add(WaitForComplete());
                cmds.Add(ProduceVacuumGCode(true)); // Turn on solenoid 
                cmds.Add(DwellGCode(250)); // Wait 500ms to pickup part.
                cmds.Add(PickHeightGCode()); // Move to pick height                
                cmds.Add(DwellGCode(250)); // Wait 500ms to pickup part.
                cmds.Add(SafeHeightGCodeGCode()); // Go to move height

                var cRotation = SelectedPartToBePlaced.RotateAngle + SelectedPartPackage.RotationInTape;
                if (cRotation > 0)
                {
                    cmds.Add(RotationGCode(cRotation));
                    cmds.Add(WaitForComplete());
                }

                cmds.Add(GetGoToPartOnBoardGCode());
                cmds.Add(PlaceHeightGCode(SelectedPartPackage));
                cmds.Add(WaitForComplete());
                cmds.Add(ProduceVacuumGCode(false));
                cmds.Add(DwellGCode(500)); // Wait 500ms to let placed part settle in
                cmds.Add(SafeHeightGCodeGCode()); // Return to move height.

                cmds.Add(RotationGCode(0)); // Ensure we are at zero position before picking up part.
                cmds.Add(WaitForComplete());

                await SendInstructionSequenceAsync(cmds);

                if (!multiple)
                {
                    Machine.Vacuum1On = false;
                    Machine.Vacuum2On = false;
                }

                SelectedPartRow.CurrentPartIndex++;
                _partIndex++;
                if (_partIndex >= SelectedPart.Parts.Count)
                {
                    SelectedPart = null;
                }
                else
                {
                    _selectPartToBePlaced = SelectedPart.Parts[_partIndex];
                }

                RaisePropertyChanged(nameof(SelectedPartToBePlaced));

                await SaveJob();

                _isPlacingParts = false;

                PlaceCurrentPartCommand.RaiseCanExecuteChanged();
                PlaceAllPartsCommand.RaiseCanExecuteChanged();
                PausePlacmentCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanAddFeeder()
        {
            return SelectedFeeder != null;
        }

        public bool CanSaveJob()
        {
            return _isDirty;
        }


        public double? RotationInTape
        {
            get
            {
                if (SelectedPart != null && SelectedPartRow != null && SelectedPartPackage != null)
                {
                    return SelectedPartPackage.RotationInTape;
                }
                else
                {
                    return null;
                }
            }
        }

        public double? XPartInTray
        {
            get
            {
                if (SelectedPart != null && SelectedPartRow != null && SelectedPartPackage != null)
                {
                    var xCorrection = SelectedPart.PartPack.CorrectionAngleX * ((SelectedPartRow.RowNumber - 1) * SelectedPart.PartPack.RowCount);
                    return SelectedPart.PartPack.Pin1XOffset + SelectedPart.Slot.X + (SelectedPartRow.CurrentPartIndex * SelectedPartPackage.SpacingX) + SelectedPartPackage.CenterXFromHole + xCorrection;
                }
                else
                {
                    return null;
                }
            }
        }

        public double? YPartInTray
        {
            get
            {
                if (SelectedPart != null && SelectedPartRow != null && SelectedPartPackage != null)
                {
                    var yCorrection = SelectedPart.PartPack.CorrectionAngleY * ((SelectedPartRow.CurrentPartIndex * SelectedPartPackage.SpacingX) + SelectedPartPackage.CenterXFromHole);


                    return SelectedPart.PartPack.Pin1YOffset + SelectedPart.Slot.Y + ((SelectedPartRow.RowNumber - 1) * SelectedPart.PartPack.RowHeight) + SelectedPartPackage.CenterYFromHole + yCorrection;
                }
                else
                {
                    return null;
                }
            }
        }

        private string GetGoToPartInTrayGCode()
        {
            return $"G0 X{XPartInTray} Y{YPartInTray} F{Machine.Settings.FastFeedRate}";
        }

        public void GoToPartPositionInTray()
        {
            if (SelectedPartPackage != null && SelectedPartRow != null)
            {
                Machine.SendCommand(SafeHeightGCodeGCode());
                Machine.SendCommand(GetGoToPartInTrayGCode());
            }
        }

        private String GetGoToPartOnBoardGCode()
        {
            return $"G1 X{SelectedPartToBePlaced.X} Y{SelectedPartToBePlaced.Y} F{Machine.Settings.FastFeedRate}";
        }

        public void GoToPartOnBoard()
        {
            if (SelectedPartToBePlaced != null)
            {
                Machine.SendCommand(SafeHeightGCodeGCode());
                Machine.SendCommand(GetGoToPartOnBoardGCode());
            }
        }

        public PartPackManagerViewModel PartPackManagerVM
        {
            get;
        }

        public PackageLibraryViewModel PackageLibraryVM
        {
            get;
        }

        PnPMachine _pnpMachine;
        public PnPMachine PnPMachine
        {
            get => _pnpMachine;
            set
            {
                Set(ref _pnpMachine, value);
                RaisePropertyChanged(nameof(Packages));
            }
        }

        public ObservableCollection<PickAndPlace.Models.Package> Packages
        {
            get { return _pnpMachine?.Packages; }
        }

        ObservableCollection<Feeder> _feederTypes;
        public ObservableCollection<Feeder> FeederTypes
        {
            get { return _feederTypes; }
            set { Set(ref _feederTypes, value); }
        }

        public ObservableCollection<FeederInstance> JobFeeders
        {
            get { return Job.Feeders; }
        }

        Feeder _selectedFeeder;
        public Feeder SelectedFeeder
        {
            get { return _selectedFeeder; }
            set
            {
                Set(ref _selectedFeeder, value);
                AddFeederCommand.RaiseCanExecuteChanged();
            }
        }

        public Row SelectedPartRow
        {
            get => SelectedPart?.Row;
        }

        public LagoVista.EaglePCB.Models.PCB Board
        {
            get { return Job.Board; }
        }

        PickAndPlace.Models.Package _selectedPartPackage;
        public PickAndPlace.Models.Package SelectedPartPackage
        {
            get => _selectedPartPackage;
            set => Set(ref _selectedPartPackage, value);
        }

        Component _selectPartToBePlaced;
        public Component SelectedPartToBePlaced
        {
            get { return _selectPartToBePlaced; }
            set
            {

                Set(ref _selectPartToBePlaced, value);

                if (value != null)
                {
                    _partIndex = SelectedPart.Parts.IndexOf(value);
                    GoToPartOnBoard();
                }
            }
        }

        public ObservableCollection<Part> Parts
        {
            get { return Job.Parts; }
        }

        public ObservableCollection<PlaceableParts> ConfigurationParts { get; } = new ObservableCollection<PlaceableParts>();

        private PlaceableParts _selectedPart;
        public PlaceableParts SelectedPart
        {
            get { return _selectedPart; }
            set
            {
                Set(ref _selectedPart, value);

                if (value != null && _pnpMachine != null)
                {
                    SelectedPartPackage = _pnpMachine.Packages.Where(pck => pck.Name == _selectedPart.Package).FirstOrDefault();
                }
                else
                {
                    SelectedPartPackage = null;
                }

                RaisePropertyChanged(nameof(SelectedPartRow));
                RaisePropertyChanged(nameof(XPartInTray));
                RaisePropertyChanged(nameof(RotationInTape));
                RaisePropertyChanged(nameof(YPartInTray));

                MoveToNextComponentInTapeCommand.RaiseCanExecuteChanged();
                MoveToPreviousComponentInTapeCommand.RaiseCanExecuteChanged();
                ResetCurrentComponentCommand.RaiseCanExecuteChanged();
                PlaceCurrentPartCommand.RaiseCanExecuteChanged();
                PlaceAllPartsCommand.RaiseCanExecuteChanged();
            }
        }

        public async void SelectMachineFile()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.PnPMachine);
            if (!String.IsNullOrEmpty(result))
            {
                try
                {
                    Job.PnPMachinePath = result;
                    PnPMachine = await PnPMachineManager.GetPnPMachineAsync(result);
                    PartPackManagerVM.SetMachine(PnPMachine);
                    await SaveJob();
                }
                catch
                {
                    await Popups.ShowAsync("Could not open packages");
                }
            }
        }


        private LagoVista.PickAndPlace.Models.PnPJob _job;
        public LagoVista.PickAndPlace.Models.PnPJob Job
        {
            get { return _job; }
            set
            {
                _isDirty = false;
                SaveCommand.RaiseCanExecuteChanged();
                Set(ref _job, value);
                RaisePropertyChanged(nameof(HasJob));
            }
        }

        public String ProgressOnPart
        {
            get
            {
                if (SelectedPart == null)
                {
                    return "-";
                }

                return $"Placing part {_partIndex} of {SelectedPart.Count}";
            }
        }

        public string FileName
        {
            get;
            set;
        }

        BuildFlavor _selectedBuildFlavor;
        public BuildFlavor SelectedBuildFlavor
        {
            get => _selectedBuildFlavor;
            set
            {
                Set(ref _selectedBuildFlavor, value);
                PopulateConfigurationParts();
            }
        }

        public ObservableCollection<BuildFlavor> BuildFlavors { get; set; } = new ObservableCollection<BuildFlavor>();

        public ObservableCollection<Component> PartsToBePlaced { get; set; } = new ObservableCollection<Component>();

        public bool HasJob { get { return Job != null; } }

        public bool IsDirty
        {
            get { return _isDirty; }
            set { Set(ref _isDirty, value); }
        }

        public bool IsEditing
        {
            get { return _isEditing; }
            set { Set(ref _isEditing, value); }
        }

        public async Task SaveJob()
        {
            if (String.IsNullOrEmpty(FileName))
            {
                FileName = await Popups.ShowSaveFileAsync(Constants.PickAndPlaceProject);
                if (String.IsNullOrEmpty(FileName))
                {
                    return;
                }
            }

            await Storage.StoreAsync(Job, FileName);

            if (!String.IsNullOrEmpty(Job.PnPMachinePath))
            {
                await PnPMachineManager.SavePackagesAsync(PnPMachine, Job.PnPMachinePath);
            }

            _isDirty = false;
            SaveCommand.RaiseCanExecuteChanged();

            SaveProfile();
        }

        public override async Task IsClosingAsync()
        {
            await SaveJob();
            await base.IsClosingAsync();
        }

        private bool _isPlacingParts = false;

        public RelayCommand HomingCycleCommand { get; }
        public RelayCommand GoToCurrentPartCommand { get; }
        public RelayCommand AddFeederCommand { get; private set; }
        public RelayCommand RefreshConfigurationPartsCommand { get; private set; }
        public RelayCommand CloneCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand GoToPartOnBoardCommand { get; private set; }
        public RelayCommand GoToPartPositionInTrayCommand { get; private set; }
        public RelayCommand SelectMachineFileCommand { get; private set; }
        public RelayCommand ResetCurrentComponentCommand { get; set; }
        public RelayCommand MoveToPreviousComponentInTapeCommand { get; set; }
        public RelayCommand PausePlacmentCommand { get; set; }
        public RelayCommand MoveToNextComponentInTapeCommand { get; set; }
        public RelayCommand GoToWorkHomeCommand { get; set; }
        public RelayCommand SetWorkHomeCommand { get; set; }
        public RelayCommand CloseCommand { get; set; }
        public RelayCommand PlaceCurrentPartCommand { get; set; }
        public RelayCommand PlaceAllPartsCommand { get; set; }
        public RelayCommand GoToPartInTrayCommand { get; }
        public RelayCommand PeformMachineAlignmentCommand { get; }
        public RelayCommand GoToFiducial1Command { get; }
        public RelayCommand GoToFiducial2Command { get; }
        public RelayCommand AlignBottomCameraCommand { get; }
        public RelayCommand CalibrateBottomCameraCommand { get; }
        public RelayCommand SetFiducialCalibrationCommand { get; }
    }
}
