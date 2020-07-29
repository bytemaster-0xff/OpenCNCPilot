using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.GCode.Sender.Interfaces;
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
    public class FeederDefinitionsViewModel : ViewModelBase
    {
        FeederLibrary _feederLibrary;

        private bool _isDirty = false;
        private bool _isEditing = false;

        private readonly IMachine _machine;

        public FeederDefinitionsViewModel(IMachine machine)
        {
            _machine = machine ?? throw new NullReferenceException(nameof(machine));

            _feederLibrary = new FeederLibrary();            
            FeederDefintions = new ObservableCollection<Feeder>();

            AddFeederCommand = new RelayCommand(AddPackage, () => CurrentFeeder == null);
            SaveFeederCommand = new RelayCommand(SaveFeeder, () => CurrentFeeder != null);
            CancelFeederCommand = new RelayCommand(CancelPackage, () => CurrentFeeder != null);
            SaveLibraryCommand = new RelayCommand(SaveLibrary, () => _isDirty == true);

            DeleteFeederCommand = new RelayCommand(DeleteFeeder, () => (CurrentFeeder != null && _isEditing == true));

            SetLocationCommand = new RelayCommand(SetLocation, () => CurrentFeeder != null);

            CurrentFeeder = null;
        }


        ObservableCollection<Feeder> _feederDefintions;
        public ObservableCollection<Feeder> FeederDefintions
        {
            get { return _feederDefintions; }
            set
            {
                Set(ref _feederDefintions, value);
            }
        }

        public override async Task InitAsync()
        {
            FeederDefintions = await _feederLibrary.GetFeedersAsync();
            await base.InitAsync();
        }

        public void AddPackage()
        {
            CurrentFeeder = new Feeder();
            _isEditing = false;
            AddFeederCommand.RaiseCanExecuteChanged();
            SaveFeederCommand.RaiseCanExecuteChanged();
            DeleteFeederCommand.RaiseCanExecuteChanged();
            CancelFeederCommand.RaiseCanExecuteChanged();
            SetLocationCommand.RaiseCanExecuteChanged();
        }
        
        public void SetLocation()
        {
            CurrentFeeder.X = _machine.MachinePosition.X;
            CurrentFeeder.Y = _machine.MachinePosition.Y;
        }


        public void CancelPackage()
        {
            CurrentFeeder = null;
        }

        public void DeleteFeeder()
        {
            _isDirty = true;
            FeederDefintions.Remove(CurrentFeeder);
            CurrentFeeder = null;
        }

        public void SaveFeeder()
        {
            if (!_isEditing)
            {
                FeederDefintions.Add(CurrentFeeder);
            }

            _isDirty = true;

            CurrentFeeder = null;
            AddFeederCommand.RaiseCanExecuteChanged();
            SaveLibraryCommand.RaiseCanExecuteChanged();
            SetLocationCommand.RaiseCanExecuteChanged();
        }

        public async void SaveLibrary()
        {
            await _feederLibrary.SaveFeederDefinitions(FeederDefintions);

            _isDirty = false;
            SaveLibraryCommand.RaiseCanExecuteChanged();
        }

        Feeder _currentPackage;
        public Feeder CurrentFeeder
        {
            get { return _currentPackage; }
            set
            {
                _isEditing = true;
                Set(ref _currentPackage, value);
                AddFeederCommand.RaiseCanExecuteChanged();
                SaveFeederCommand.RaiseCanExecuteChanged();
                DeleteFeederCommand.RaiseCanExecuteChanged();
                CancelFeederCommand.RaiseCanExecuteChanged();
                SetLocationCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand AddFeederCommand { get; private set; }
        public RelayCommand OpenLibraryCommand { get; private set; }
        public RelayCommand SaveLibraryCommand { get; private set; }

        public RelayCommand SaveFeederCommand { get; private set; }
        public RelayCommand DeleteFeederCommand { get; private set; }
        public RelayCommand CancelFeederCommand { get; private set; }

        public RelayCommand SetLocationCommand { get; private set; }
    }
}
