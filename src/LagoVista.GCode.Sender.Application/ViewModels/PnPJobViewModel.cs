using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.ViewModels;
using LagoVista.EaglePCB.Models;
using LagoVista.GCode.Sender;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.Repos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public class PnPJobViewModel : MachineVisionViewModelBase
    {
        private bool _isEditing;
        private bool _isDirty = false;
        private BOM _billOfMaterials;
        PackageLibrary _packageLibrary;
        private FeederLibrary _feederLibrary;

        private int _partIndex = -1;


        public PnPJobViewModel(IMachine machine, PnPJob job) : base(machine)
        {
            _billOfMaterials = new BOM(job.Board);
            _job = job;
            _isDirty = true;

            AddFeederCommand = new RelayCommand(AddFeeder, () => CanAddFeeder());
            SaveJobCommand = new RelayCommand(SaveJob, () => CanSaveJob());

            GoToPartOnBoardCommand = new RelayCommand(GoToPartOnBoard);
            GoToPartPositionInTrayCommand = new RelayCommand(GoToPartPositionInTray);

            SelectPackagesFileCommand = new RelayCommand(SelectPackagesFile);

            ResetCurrentComponentCommand = new RelayCommand(ResetCurrentComponent, () => SelectedPartRow != null);

            GoToWorkHomeCommand = new RelayCommand(() => Machine.GotoWorkspaceHome());
            SetWorkHomeCommand = new RelayCommand(() => Machine.SetWorkspaceHome());

            MoveToPreviousComponentInTapeCommand = new RelayCommand(MoveToPreviousComponent, () => SelectedPartRow != null && SelectedPartRow.CurrentPartIndex > 0);
            MoveToNextComponentInTapeCommand = new RelayCommand(MoveToNextComponentInTape, () => SelectedPartRow != null && SelectedPartRow.CurrentPartIndex < SelectedPartRow.PartCount);
            SetFirstComponentLocationCommand = new RelayCommand(SetFirstComponentLocation, () => SelectedPartRow != null);
            GoToFirstComponentReferenceCommand = new RelayCommand(GoToFirstComponentReference, () => SelectedPartRow != null);
            PlacePartCommand = new RelayCommand(PlacePart, () => SelectedPart != null);
            _feederLibrary = new FeederLibrary();
            _packageLibrary = new PackageLibrary();

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

        public override async Task InitAsync()
        {
            await base.InitAsync();
            FeederTypes = await _feederLibrary.GetFeedersAsync();
            LoadingMask = false;

            if (!String.IsNullOrEmpty(_job.PackagesPath) && System.IO.File.Exists(_job.PackagesPath))
            {
                _packages = await _packageLibrary.GetPackagesAsync(_job.PackagesPath);
            }
        }

        public void GoToFirstComponentReference()
        {
            if (SelectedPartRow != null)
            {
                Machine.SendCommand($"G0 X{SelectedPartRow.FirstComponentX} Y{SelectedPartRow.FirstComponentY}");
            }
        }

        public void ResetCurrentComponent()
        {
            SelectedPartRow.CurrentPartIndex = 0;
            GoToPartPositionInTray();
            SaveJob();
        }

        public void MoveToPreviousComponent()
        {
            if (SelectedPartRow.CurrentPartIndex > 0)
            {
                SelectedPartRow.CurrentPartIndex--;
                MoveToNextComponentInTapeCommand.RaiseCanExecuteChanged();
                MoveToPreviousComponentInTapeCommand.RaiseCanExecuteChanged();
                ResetCurrentComponentCommand.RaiseCanExecuteChanged();
                SaveJob();
                GoToPartPositionInTray();
            }
        }

        public void MoveToNextComponentInTape()
        {
            if (SelectedPartRow.CurrentPartIndex < SelectedPartRow.PartCount)
            {
                SelectedPartRow.CurrentPartIndex++;
                MoveToNextComponentInTapeCommand.RaiseCanExecuteChanged();
                MoveToPreviousComponentInTapeCommand.RaiseCanExecuteChanged();
                ResetCurrentComponentCommand.RaiseCanExecuteChanged();
                SaveJob();
                GoToPartPositionInTray();
            }
        }

        public void SetFirstComponentLocation()
        {
            SelectedPartRow.FirstComponentX = Machine.MachinePosition.X;
            SelectedPartRow.FirstComponentY = Machine.MachinePosition.Y;
            MoveToNextComponentInTapeCommand.RaiseCanExecuteChanged();
            MoveToPreviousComponentInTapeCommand.RaiseCanExecuteChanged();
            ResetCurrentComponentCommand.RaiseCanExecuteChanged();
            SaveJob();
        }

        async Task SendInstructionSequenceAsync(List<string> cmds)
        {
            foreach (var cmd in cmds)
            {
                Machine.SendCommand(cmd);

                while (Machine.Busy) await Task.Delay(1);
            }
        }

        public async void PlacePart()
        {
            if (_partIndex < PartsToBePlaced.Count - 1)
            {
                if (!Machine.Vacuum1On || !Machine.Vacuum2On)
                {
                    Machine.Vacuum1On = true;
                    Machine.Vacuum2On = true;
                    await Task.Delay(2000);
                }

                _partIndex++;
                _selectPartToBePlaced = PartsToBePlaced[_partIndex];
                SelectedPartRow.CurrentPartIndex++;
                SaveJob();
                RaisePropertyChanged(nameof(SelectedPartToBePlaced));
                GoToPartPositionInTray();

                var cmds = new List<string>();
                cmds.Add("M54"); // Move to move height
                cmds.Add(GetGoToPartInTrayGCode());                
                cmds.Add("M55"); // Move to pick height
                cmds.Add("M64 P255"); // Turn on solenoid 
                cmds.Add("G4 P500"); // Wait 500ms to pickup part.
                cmds.Add("M54"); // Go to move height
                cmds.Add(GetGoToPartOnBoardGCode());
                cmds.Add("M56"); // Go to place height
                cmds.Add("G4 P100"); // Wait 100ms to before turning off solenoid
                cmds.Add("M64 P0");
                cmds.Add("G4 P500"); // Wait 500ms to let placed part settle in
                cmds.Add("M54"); // Return to move height.

                await SendInstructionSequenceAsync(cmds);
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

        public void AddFeeder()
        {
            var feederInstance = new FeederInstance();
            feederInstance.SetFeeder(SelectedFeeder);
            JobFeeders.Add(feederInstance);
            feederInstance.Name = $"Feeder - {JobFeeders.Count}";
            SelectedFeederInstance = feederInstance;
            SelectedFeeder = null;
        }

        private string GetGoToPartInTrayGCode()
        {
            var offsetY = SelectedPartRow.FirstComponentY + SelectedPartPackage.CenterYFromHole;
            var offsetX = SelectedPartRow.CurrentPartIndex * SelectedPartPackage.HoleSpacing + SelectedPartPackage.CenterXFromHole + SelectedPartRow.FirstComponentX;
            return $"G0 X{offsetX} Y{offsetY}";
        }

        public void GoToPartPositionInTray()
        {
            if (SelectedPartPackage != null && SelectedPartRow != null)
            {
                Machine.SendCommand(GetGoToPartInTrayGCode()); ;
            }
        }

        private String GetGoToPartOnBoardGCode()
        {
            return $"G1 X{SelectedPartToBePlaced.X} Y{SelectedPartToBePlaced.Y} F500";
        }

        public void GoToPartOnBoard()
        {
            if (SelectedPartToBePlaced != null)
            {
                Machine.SendCommand(GetGoToPartOnBoardGCode());
            }
        }

        ObservableCollection<PickAndPlace.Models.Package> _packages;
        public ObservableCollection<PickAndPlace.Models.Package> Packages
        {
            get { return _packages; }
            set { Set(ref _packages, value); }
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

        FeederInstance _selectedFeederInstance;
        public FeederInstance SelectedFeederInstance
        {
            get { return _selectedFeederInstance; }
            set { Set(ref _selectedFeederInstance, value); }
        }

        public string SelectedPackageId
        {
            get { return SelectedPart?.PackageId; }
            set
            {
                if (SelectedPart != null && value != null)
                {
                    SelectedPart.PackageId = value;
                    _isDirty = true;
                    SaveJobCommand.RaiseCanExecuteChanged();

                }

                RaisePropertyChanged(nameof(SelectedPartPackage));
                RaisePropertyChanged();
            }
        }

        public PickAndPlace.Models.Package SelectedPartPackage
        {
            get
            {
                if (SelectedPart != null)
                {
                    return Packages.Where(pck => pck.Id == SelectedPart.PackageId).FirstOrDefault();
                }
                return null;
            }
        }

        public FeederInstance SelectedPartFeeder
        {
            get
            {
                if (SelectedPart != null)
                {
                    return JobFeeders.Where(fdr => fdr.Feeder.Id == SelectedPart.FeederId).FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (SelectedPart != null && value != null)
                {
                    SelectedPart.FeederId = value.Feeder.Id;
                    _isDirty = true;
                    SaveJobCommand.RaiseCanExecuteChanged();
                }

                RaisePropertyChanged();
            }
        }

        public Row SelectedPartRow
        {
            get
            {
                if (SelectedPart == null || !SelectedPart.RowNumber.HasValue)
                {
                    return null;
                }
                else
                {
                    var feeder = Job.Feeders.Where(fdr => fdr.Feeder.Id == SelectedPart.FeederId).FirstOrDefault();
                    if (SelectedPart.RowNumber.Value < feeder.Rows.Count)
                    {
                        return feeder.Rows[SelectedPart.RowNumber.Value - 1];
                    }
                    else
                    {
                        return null;
                    }

                }
            }
            set
            {
                if (SelectedPart != null && value != null)
                {
                    SelectedPart.RowNumber = value.RowNumber;
                    value.Part = SelectedPart;
                    _isDirty = true;
                    SaveJobCommand.RaiseCanExecuteChanged();
                }
                RaisePropertyChanged();

                _partIndex = -1;

                MoveToNextComponentInTapeCommand.RaiseCanExecuteChanged();
                MoveToPreviousComponentInTapeCommand.RaiseCanExecuteChanged();
                ResetCurrentComponentCommand.RaiseCanExecuteChanged();
                SetFirstComponentLocationCommand.RaiseCanExecuteChanged();
            }
        }

        public LagoVista.EaglePCB.Models.PCB Board
        {
            get { return Job.Board; }
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

        public BOM BillOfMaterials
        {
            get { return _billOfMaterials; }
        }

        public ObservableCollection<Part> Parts
        {
            get { return Job.Parts; }
        }


        private Part _selectedPart;
        public Part SelectedPart
        {
            get { return _selectedPart; }
            set
            {
                Set(ref _selectedPart, value);
                RaisePropertyChanged(nameof(SelectedPartFeeder));
                RaisePropertyChanged(nameof(SelectedPartRow));
                RaisePropertyChanged(nameof(SelectedPackageId));

                MoveToNextComponentInTapeCommand.RaiseCanExecuteChanged();
                MoveToPreviousComponentInTapeCommand.RaiseCanExecuteChanged();
                ResetCurrentComponentCommand.RaiseCanExecuteChanged();
                SetFirstComponentLocationCommand.RaiseCanExecuteChanged();
                PlacePartCommand.RaiseCanExecuteChanged();

                PartsToBePlaced.Clear();
                var partsOfType = _billOfMaterials.SMDEntries.Where(ent =>
                   value.LibraryName == ent.Package.LibraryName &&
                        value.PackageName == ent.Package.Name &&
                        value.Value == ent.Value);

                foreach (var part in partsOfType)
                {
                    foreach (var component in part.Components)
                        PartsToBePlaced.Add(component);
                }
            }
        }

        public async void SelectPackagesFile()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.FileFilterPCB);
            if (!String.IsNullOrEmpty(result))
            {
                try
                {
                    Job.PackagesPath = result;
                    Packages = await _packageLibrary.GetPackagesAsync(result);
                    SaveJob();
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
                SaveJobCommand.RaiseCanExecuteChanged();
                Set(ref _job, value);
                RaisePropertyChanged(nameof(HasJob));
            }
        }

        public string FileName
        {
            get;
            set;
        }

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

        public async void SaveJob()
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

            _isDirty = false;
            SaveJobCommand.RaiseCanExecuteChanged();
        }


        public RelayCommand AddFeederCommand { get; private set; }

        public RelayCommand SaveJobCommand { get; private set; }

        public RelayCommand GoToPartOnBoardCommand { get; private set; }

        public RelayCommand GoToPartPositionInTrayCommand { get; private set; }

        public RelayCommand SelectPackagesFileCommand { get; private set; }

        public RelayCommand ResetCurrentComponentCommand { get; set; }
        public RelayCommand MoveToPreviousComponentInTapeCommand { get; set; }
        public RelayCommand MoveToNextComponentInTapeCommand { get; set; }
        public RelayCommand SetFirstComponentLocationCommand { get; set; }
        public RelayCommand GoToFirstComponentReferenceCommand { get; set; }

        public RelayCommand GoToWorkHomeCommand { get; set; }
        public RelayCommand SetWorkHomeCommand { get; set; }

        public RelayCommand PlacePartCommand { get; set; }
    }
}
