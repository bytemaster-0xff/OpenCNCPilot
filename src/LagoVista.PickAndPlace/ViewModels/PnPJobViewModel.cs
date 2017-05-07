using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.EaglePCB.Models;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.Repos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels
{
    public class PnPJobViewModel : ViewModelBase
    {
        private bool _isEditing;
        private bool _isDirty;

        private PCB _board;
        private BOM _billOfMaterials;

        private FeederLibrary _feederLibrary;

        public PnPJobViewModel(PCB pcb, Models.PnPJob job)
        {
            _board = pcb;
            _billOfMaterials = new BOM(pcb);
            _job = job;

            _feederLibrary = new FeederLibrary();

            FeederTypes = new ObservableCollection<Feeder>();

            AddFeederTypeFileCommand = new RelayCommand(AddFeederTypeFile);


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
        

        ObservableCollection<Feeder> _feederTypes;
        public ObservableCollection<Feeder> FeederTypes
        {
            get { return Job.Feeders; }
            set { Set(ref _feeders, value); }
        }

        ObservableCollection<Feeder> _feeders;
        public ObservableCollection<Feeder> Feeders
        {
            get { return Job.Feeders; }
        }

        Feeder _selectedFeeder;
        public Feeder SelectedFeeder
        {
            get { return _selectedFeeder; }
            set { Set(ref _selectedFeeder, value); }
        }

        public PCB Board
        {
            get { return _board; }
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
            }
        }

        private Models.PnPJob _job;
        public Models.PnPJob Job
        {
            get { return _job; }
            set
            {
                Set(ref _job, value);
                RaisePropertyChanged(nameof(HasJob));
            }
        }

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

        public void SaveJob()
        {
            if (!_isEditing)
            {

            }

            _isDirty = true;

        }

        public void OpenJob()
        {

        }

        public async void AddFeederTypeFile()
        {
            var fileName = await Popups.ShowOpenFileAsync("Feeder Libraries (*.fdrs) | *.fdrs");
            if (!String.IsNullOrEmpty(fileName))
            {
                var feeders = await _feederLibrary.GetFeederDefinitions(fileName);
                foreach(var feeder in feeders)
                {
                    if(!FeederTypes.Where(fdr=>fdr.Id == feeder.Id).Any())
                    {
                        FeederTypes.Add(feeder);
                    }
                }
            }
        }

        public RelayCommand OpenJobCommand { get; private set; }

        public RelayCommand SaveJobCommand { get; private set; }    

        public RelayCommand AddFeederTypeFileCommand { get; private set; }
    }
}
