﻿using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.ViewModels;
using LagoVista.EaglePCB.Models;
using LagoVista.GCode.Sender;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.PickAndPlace.Managers;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.Repos;
using LagoVista.PickAndPlace.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public class PnPJobViewModel : MachineVisionViewModelBase
    {
        private bool _isEditing;
        private bool _isDirty = false;
        private BOM _billOfMaterials;
        private FeederLibrary _feederLibrary;

        private int _partIndex = 0;

        private enum MVLocatorState
        {
            Idle,
            BoardFidicual1,
            BoardFidicual2,
            Default,
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

            PeformBoardAlignmentCommand = new RelayCommand(PerformBoardAlignment);

            GoToPartOnBoardCommand = new RelayCommand(GoToPartOnBoard);
            GoToPartPositionInTrayCommand = new RelayCommand(GoToPartPositionInTray);

            SelectMachineFileCommand = new RelayCommand(SelectMachineFile);

            ResetCurrentComponentCommand = new RelayCommand(ResetCurrentComponent, () => SelectedPartRow != null);

            GoToWorkHomeCommand = new RelayCommand(() => Machine.GotoWorkspaceHome());
            SetWorkHomeCommand = new RelayCommand(() => Machine.SetWorkspaceHome());

            MoveToPreviousComponentInTapeCommand = new RelayCommand(MoveToPreviousComponent, () => SelectedPartRow != null && SelectedPartRow.CurrentPartIndex > 0);
            MoveToNextComponentInTapeCommand = new RelayCommand(MoveToNextComponentInTape, () => SelectedPartRow != null && SelectedPartRow.CurrentPartIndex < SelectedPartRow.PartCount);
            RefreshConfigurationPartsCommand = new RelayCommand(PopulateConfigurationParts);
            GoToPartInTrayCommand = new RelayCommand(GoToPartPositionInTray);

            PlacePartCommand = new RelayCommand(PlacePart, () => SelectedPart != null);
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

        private void GoToFiducial(int idx)
        {
            switch (idx)
            {
                case 1:
                    {
                        var gcode = $"G1 X{Job.BoardFiducial1.X} Y{Job.BoardFiducial1.Y}";
                        Machine.SendCommand(gcode);
                    }
                    break;
                case 2:
                    {
                        var gcode = $"G1 X{Job.BoardFiducial2.X} Y{Job.BoardFiducial2.Y}";
                        Machine.SendCommand(gcode);
                    }
                    break;
            }
        }

        public async void PerformBoardAlignment()
        {
            _mvLocatorState = MVLocatorState.Idle;

            var waiter = new SpinWait();
            Machine.GotoWorkspaceHome();
            while (Machine.Busy)
            {
                waiter.SpinOnce();
            }

            var gcode = $"G1 X{Job.BoardFiducial1.X} Y{Job.BoardFiducial1.Y}";
            Machine.SendCommand(gcode);

            while (Machine.Busy)
            {
                waiter.SpinOnce();
            }

            SelectMVProfile("brdfiducual");
            ShowCircles = true;

            _mvLocatorState = MVLocatorState.BoardFidicual1;
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
                ConfigurationParts.Add(new PlaceableParts()
                {
                    Count = entry.Count(),
                    Value = entry.First().Value.ToUpper(),
                    Package = entry.First().PackageName.ToUpper(),
                    Parts = new ObservableCollection<Component>(SelectedBuildFlavor.Components.Where(cmp => cmp.Key == entry.Key))
                });
            }

            if (_pnpMachine != null)
            {
                foreach (var part in ConfigurationParts)
                {
                    PnPMachineManager.ResolvePart(_pnpMachine, part);
                }
            }
        }

        public override void CircleLocated(Point2D<double> point, double diameter, Point2D<double> stdDeviation)
        {
            switch (_mvLocatorState)
            {
                case MVLocatorState.BoardFidicual1:
                    JogToLocation(point);
                    break;
                case MVLocatorState.BoardFidicual2:
                    JogToLocation(point);
                    break;
                default:
                    if (PartPackManagerVM.IsLocating)
                    {
                        JogToLocation(point);
                    }
                    break;
            }
        }

        private async void SetNewHome()
        {
            var newX = Machine.MachinePosition.X - Job.BoardFiducial1.X;
            var newY = Machine.MachinePosition.Y - Job.BoardFiducial1.Y;

            Machine.GotoPoint(newX, newY);

            await Task.Delay(2000);

            Machine.SetWorkspaceHome();

            await Task.Delay(1000);

            var gcode = $"G1 X{Job.BoardFiducial2.X} Y{Job.BoardFiducial2.Y}";
            Machine.SendCommand(gcode);

            ShowCircles = false;

            _mvLocatorState = MVLocatorState.Default;
        }

        public override void CircleCentered(Point2D<double> point, double diameter)
        {

            switch (_mvLocatorState)
            {
                case MVLocatorState.BoardFidicual1:
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

        public override async Task InitAsync()
        {
            await base.InitAsync();
            FeederTypes = await _feederLibrary.GetFeedersAsync();
            LoadingMask = false;

            if (!String.IsNullOrEmpty(_job.PnPMachinePath) && System.IO.File.Exists(_job.PnPMachinePath))
            {
                PnPMachine = await PnPMachineManager.GetPnPMachineAsync(_job.PnPMachinePath);
                PartPackManagerVM.SetMachine(PnPMachine);
                PackageLibraryVM.SetMachine(PnPMachine);
            }
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
                RaisePropertyChanged(nameof(YPartInTray));
                RaisePropertyChanged(nameof(SelectedPartRow));
                await SaveJob();
                GoToPartPositionInTray();
            }
        }

        async Task SendInstructionSequenceAsync(List<string> cmds)
        {
            foreach (var cmd in cmds)
            {
                Machine.SendCommand(cmd);
                await Task.Delay(50);
                while (Machine.Busy) await Task.Delay(1);
            }
        }

        public string SafeHeightGCode()
        {
            return "G0 Z1";
        }

        public async void PlacePart()
        {
            if (_partIndex < SelectedPart.Parts.Count)
            {
                if (!Machine.Vacuum1On || !Machine.Vacuum2On)
                {
                    Machine.Vacuum1On = true;
                    Machine.Vacuum2On = true;
                    await Task.Delay(2000);
                }

                _selectPartToBePlaced = SelectedPart.Parts[_partIndex];
                RaisePropertyChanged(nameof(SelectedPartToBePlaced));

                var cmds = new List<string>();
                cmds.Add("M54"); // Move to move height
                cmds.Add(GetGoToPartInTrayGCode());

                cmds.Add("M55"); // Move to pick height
                cmds.Add("M64 P255"); // Turn on solenoid 
                cmds.Add("G4 P2500"); // Wait 500ms to pickup part.
                cmds.Add("M54"); // Go to move height


                var cRotation = SelectedPartToBePlaced.RotateAngle + SelectedPartPackage.RotationInTape;
                if (cRotation > 360)
                    cRotation -= 360;

                if (cRotation == 270)
                    cRotation = -90;

                if (cRotation != 0)
                {
                    // rotation is sorta unique in that it is relative, there fore
                    // to ensure that we will do the required rotation, we reset the 
                    // rotation to ensure it's at zero.  Probably a better approach
                    // could be done with different firmare.
                    cmds.Add("G0 C0 F250");
                    cmds.Add($"G0 C{cRotation} F300"); // Go to place height
                }

                cmds.Add(GetGoToPartOnBoardGCode());
                cmds.Add("M56"); // Go to place height
                cmds.Add("G4 P100"); // Wait 100ms to before turning off solenoid
                cmds.Add("M64 P0");
                cmds.Add("G4 P500"); // Wait 500ms to let placed part settle in
                cmds.Add("M54"); // Return to move height.

                SelectedPartRow.CurrentPartIndex++;
                _partIndex++;

                await SaveJob();

                Task.Run(async () =>
                {
                    await SendInstructionSequenceAsync(cmds);
                });
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

        public double? XPartInTray
        {
            get
            {
                if (SelectedPart != null && SelectedPartRow != null && SelectedPartPackage != null)
                {
                    return SelectedPart.PartPack.Pin1XOffset + SelectedPart.Slot.X + (SelectedPartRow.CurrentPartIndex * SelectedPartPackage.SpacingX) + SelectedPartPackage.CenterXFromHole;
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
                    return SelectedPart.PartPack.Pin1YOffset + SelectedPart.Slot.Y + ((SelectedPartRow.RowNumber - 1) * SelectedPart.PartPack.RowHeight) + SelectedPartPackage.CenterYFromHole;
                }
                else
                {
                    return null;
                }
            }
        }

        private string GetGoToPartInTrayGCode()
        {
            return $"G0 X{XPartInTray} Y{YPartInTray} F1250";
        }

        public void GoToPartPositionInTray()
        {
            if (SelectedPartPackage != null && SelectedPartRow != null)
            {
                Machine.SendCommand(GetGoToPartInTrayGCode());
            }
        }

        private String GetGoToPartOnBoardGCode()
        {
            return $"G1 X{SelectedPartToBePlaced.X} Y{SelectedPartToBePlaced.Y} F1250";
        }

        public void GoToPartOnBoard()
        {
            if (SelectedPartToBePlaced != null)
            {
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
                GoToPartOnBoard();
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

                if (value != null)
                {
                    SelectedPartPackage = _pnpMachine.Packages.Where(pck => pck.Name == _selectedPart.Package).FirstOrDefault();
                }
                else
                {
                    SelectedPartPackage = null;
                }

                RaisePropertyChanged(nameof(SelectedPartRow));
                RaisePropertyChanged(nameof(XPartInTray));
                RaisePropertyChanged(nameof(YPartInTray));

                MoveToNextComponentInTapeCommand.RaiseCanExecuteChanged();
                MoveToPreviousComponentInTapeCommand.RaiseCanExecuteChanged();
                ResetCurrentComponentCommand.RaiseCanExecuteChanged();
                PlacePartCommand.RaiseCanExecuteChanged();
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
        public RelayCommand MoveToNextComponentInTapeCommand { get; set; }

        public RelayCommand GoToWorkHomeCommand { get; set; }
        public RelayCommand SetWorkHomeCommand { get; set; }

        public RelayCommand CloseCommand { get; set; }

        public RelayCommand PlacePartCommand { get; set; }

        public RelayCommand GoToPartInTrayCommand { get; }

        public RelayCommand PeformBoardAlignmentCommand { get; }

        public RelayCommand GoToFiducial1Command { get; }
        public RelayCommand GoToFiducial2Command { get; }
    }
}
