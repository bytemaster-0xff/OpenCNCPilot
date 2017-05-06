using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
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

        public FeederDefinitionsViewModel()
        {
            _feederLibrary = new FeederLibrary();
            FeederDefintions = new ObservableCollection<Feeder>();

            AddFeederCommand = new RelayCommand(AddPackage, () => CurrentFeeder == null);
            SaveFeederCommand = new RelayCommand(SavePackage, () => CurrentFeeder != null);
            CancelFeederCommand = new RelayCommand(CancelPackage, () => CurrentFeeder != null);

            SaveLibraryCommand = new RelayCommand(SaveLibrary, () => _isDirty == true);
            OpenLibraryCommand = new RelayCommand(OpenLibrary);
            DeletePackageCommand = new RelayCommand(DeletePackage, () => (CurrentFeeder != null && _isEditing == true));

            CurrentFeeder = null;
        }

        private string _fileName;

        ObservableCollection<Feeder> _feederDefintions;
        public ObservableCollection<Feeder> FeederDefintions
        {
            get { return _feederDefintions; }
            set
            {
                Set(ref _feederDefintions, value);
            }
        }

        public void AddPackage()
        {
            CurrentFeeder = new Feeder();
            _isEditing = false;
            AddFeederCommand.RaiseCanExecuteChanged();
            SaveFeederCommand.RaiseCanExecuteChanged();
            DeletePackageCommand.RaiseCanExecuteChanged();
            CancelFeederCommand.RaiseCanExecuteChanged();
        }

        public async void OpenLibrary()
        {
            if (CurrentFeeder != null || _isDirty)
            {
                if (await Popups.ConfirmAsync("Lose Changes?", "You have unsaved work, opening a new file will cause you to lose changes.\r\n\r\nContinue?"))
                {
                    return;
                }
            }

            var fileName = await Popups.ShowOpenFileAsync("Feeder Libraries (*.fdrs) | *.fdrs");
            if (!String.IsNullOrEmpty(fileName))
            {
                FeederDefintions = await _feederLibrary.GetFeederDefinitions(fileName);
            }
        }

        public void CancelPackage()
        {
            CurrentFeeder = null;
        }

        public void DeletePackage()
        {
            _isDirty = true;
            FeederDefintions.Remove(CurrentFeeder);
            CurrentFeeder = null;
        }

        public void SavePackage()
        {
            if (!_isEditing)
            {
                FeederDefintions.Add(CurrentFeeder);
            }

            _isDirty = true;

            CurrentFeeder = null;
            AddFeederCommand.RaiseCanExecuteChanged();
            SaveLibraryCommand.RaiseCanExecuteChanged();
        }

        public async void SaveLibrary()
        {
            if (String.IsNullOrEmpty(_fileName))
            {
                _fileName = await Popups.ShowSaveFileAsync("Feeder Libraries (*.fdrs) | *.fdrs");
                if (String.IsNullOrEmpty(_fileName))
                {
                    return;
                }
            }

            await _feederLibrary.SaveFeederDefinitions(FeederDefintions, _fileName);

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
                DeletePackageCommand.RaiseCanExecuteChanged();
                CancelFeederCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand AddFeederCommand { get; private set; }
        public RelayCommand OpenLibraryCommand { get; private set; }
        public RelayCommand SaveLibraryCommand { get; private set; }

        public RelayCommand SaveFeederCommand { get; private set; }
        public RelayCommand DeletePackageCommand { get; private set; }
        public RelayCommand CancelFeederCommand { get; private set; }
    }
}
