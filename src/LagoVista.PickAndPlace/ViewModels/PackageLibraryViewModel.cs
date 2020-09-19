using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Managers;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.Repos;
using System;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.ViewModels
{
    public class PackageLibraryViewModel : ViewModelBase
    {
        private PnPMachine _pnpMachine;

        private bool _isDirty = false;
        private bool _isEditing = false;
        private string _fileName;

        public PackageLibraryViewModel()
        {
            NewMachineCommand = new RelayCommand(NewMachine);

            AddPackageCommand = new RelayCommand(AddPackage, () => CurrentPackage == null);
            SavePackageCommand = new RelayCommand(SavePackage, () => CurrentPackage != null);
            CancelPackageCommand = new RelayCommand(CancelPackage, () => CurrentPackage != null);

            SaveMachineCommand = new RelayCommand(SaveMachine, () => _isDirty == true);
            OpenMachineCommand = new RelayCommand(OpenMachine);
            DeletePackageCommand = new RelayCommand(DeletePackage, () => (CurrentPackage != null && _isEditing == true));

            CurrentPackage = null;
        }

        public ObservableCollection<Package> Packages
        {
            get { return _pnpMachine?.Packages; }
        }

        public void AddPackage()
        {
            CurrentPackage = new Package();
            _isEditing = false;
            AddPackageCommand.RaiseCanExecuteChanged();
            SavePackageCommand.RaiseCanExecuteChanged();
            DeletePackageCommand.RaiseCanExecuteChanged();
            CancelPackageCommand.RaiseCanExecuteChanged();
        }

        public async void NewMachine()
        {
            if (CurrentPackage != null || _isDirty)
            {
                if (!await Popups.ConfirmAsync("Lose Changes?", "You have unsaved work, opening a new file will cause you to lose changes.\r\n\r\nContinue?"))
                {
                    return;
                }
            }

            _isDirty = true;
            _isEditing = false;
            _pnpMachine = new PnPMachine();

            RaisePropertyChanged(nameof(Packages));
            SaveMachineCommand.RaiseCanExecuteChanged();
        }

        public async void OpenMachine()
        {
            if (CurrentPackage != null || _isDirty)
            {
                if (!await Popups.ConfirmAsync("Lose Changes?", "You have unsaved work, opening a new file will cause you to lose changes.\r\n\r\nContinue?"))
                {
                    return;
                }
            }

            var fileName = await Popups.ShowOpenFileAsync("PnP Machine (*.pnp)|*.pnp");
            if (!String.IsNullOrEmpty(fileName))
            {
                _pnpMachine = await PnPMachineManager.GetPnPMachineAsync(fileName);
                RaisePropertyChanged(nameof(Packages));
            }
        }

        public void SetMachine(PnPMachine machine)
        {
            _pnpMachine = machine;
            RaisePropertyChanged(nameof(Packages));
        }

        public void CancelPackage()
        {
            CurrentPackage = null;
        }

        public void DeletePackage()
        {
            _isDirty = true;
            Packages.Remove(CurrentPackage);
            CurrentPackage = null;
        }

        public void SavePackage()
        {
            if (!_isEditing)
            {
                Packages.Add(CurrentPackage);
            }

            _isDirty = true;

            CurrentPackage = null;
            AddPackageCommand.RaiseCanExecuteChanged();
            SaveMachineCommand.RaiseCanExecuteChanged();
        }

        public async void SaveMachine()
        {
            if (String.IsNullOrEmpty(_fileName))
            {
                _fileName = await Popups.ShowSaveFileAsync("PnP Machine (*.pnp)|*.pnp");
                if (String.IsNullOrEmpty(_fileName))
                {
                    return;
                }
            }

            await PnPMachineManager.SavePackagesAsync(_pnpMachine, _fileName);

            _isDirty = false;
            SaveMachineCommand.RaiseCanExecuteChanged();
        }

        Package _currentPackage;
        public Package CurrentPackage
        {
            get { return _currentPackage; }
            set
            {
                _isEditing = true;
                Set(ref _currentPackage, value);
                AddPackageCommand.RaiseCanExecuteChanged();
                SavePackageCommand.RaiseCanExecuteChanged();
                DeletePackageCommand.RaiseCanExecuteChanged();
                CancelPackageCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand AddPackageCommand { get; private set; }
        public RelayCommand NewMachineCommand { get; private set; }
        public RelayCommand OpenMachineCommand { get; private set; }
        public RelayCommand SaveMachineCommand { get; private set; }

        public RelayCommand SavePackageCommand { get; private set; }
        public RelayCommand DeletePackageCommand { get; private set; }
        public RelayCommand CancelPackageCommand { get; private set; }
    }
}
