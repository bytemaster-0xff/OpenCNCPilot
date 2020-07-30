using LagoVista.Core.Commanding;
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

        private FeederLibrary _feederLibrary;

        public PnPJobViewModel(IMachine machine, PnPJob job) : base(machine)
        {
            _billOfMaterials = new BOM(job.Board);
            _job = job;
            _isDirty = true;

            AddFeederCommand = new RelayCommand(AddFeeder, () => CanAddFeeder());
            SaveJobCommand = new RelayCommand(SaveJob, () => CanSaveJob());
            _feederLibrary = new FeederLibrary();

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
            FeederTypes = await _feederLibrary.GetFeedersAsync();
            LoadingMask = false;
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
            }
        }

        public LagoVista.EaglePCB.Models.PCB Board
        {
            get { return Job.Board; }
        }

        Component _selectPartToBePlaced;
        public Component SelectPartToBePlaced
        {
            get { return _selectPartToBePlaced; }
            set { Set(ref _selectPartToBePlaced, value);  }
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
    }
}
