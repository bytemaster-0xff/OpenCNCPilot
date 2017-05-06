using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.EaglePCB.Models;
using LagoVista.PickAndPlace.Models;
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

        private ObservableCollection<Models.Package> _packages;
        private ObservableCollection<Models.Feeder> _availableFeeders;

        public PnPJobViewModel(PCB pcb, Models.PnPJob job, Models.Package packages)
        {
            _board = pcb;
            _billOfMaterials = new BOM(pcb);
            _job = job;

            foreach (var entry in _billOfMaterials.Entries)
            {
                if (!Parts.Where(prt => prt.PackageName == entry.Package.Name &&
                                        prt.LibraryName == entry.Package.LibraryName &&
                                        prt.Value == entry.Value).Any())
                {
                    Parts.Add(new Part()
                    {
                        LibraryName = entry.Package.LibraryName,
                        PackageName = entry.Package.Name,
                        Value = entry.Value
                    });
                }
            }
        }

        public void Init()
        {

        }

        public PCB Board
        {
            get { return _board; }
        }

        public BOM BillOfMaterials
        {
            get { return _billOfMaterials; }
        }

        public List<Part> Parts
        {
            get { return Job.Parts; }
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

        public RelayCommand OpenJobCommand { get; private set; }

        public RelayCommand SaveJobCommand { get; private set; }    
    }
}
