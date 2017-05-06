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
    public class PackageLibraryViewModel : ViewModelBase
    {
        PackageLibrary _packageLibrary;

        private bool _isDirty = false;
        private bool _isEditing = false;

        public PackageLibraryViewModel()
        {
            _packageLibrary = new PackageLibrary();
            Packages = new ObservableCollection<Package>();

            AddPackageCommand = new RelayCommand(AddPackage, () => CurrentPackage == null);
            SavePackageCommand = new RelayCommand(SavePackage, () => CurrentPackage != null);
            CancelPackageCommand = new RelayCommand(CancelPackage, () => CurrentPackage != null);

            SaveLibraryCommand = new RelayCommand(SaveLibrary, () => _isDirty == true);
            OpenLibraryCommand = new RelayCommand(OpenLibrary);
            DeletePackageCommand = new RelayCommand(DeletePackage, () => (CurrentPackage != null && _isEditing == true));

            CurrentPackage = null;
        }

        private string _fileName;

        ObservableCollection<Package> _packages;
        public ObservableCollection<Package> Packages
        {
            get { return _packages; }
            set
            {
                Set(ref _packages, value);
            }
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

        public async void OpenLibrary()
        {
            if(CurrentPackage != null || _isDirty)
            {
                if(await Popups.ConfirmAsync("Lose Changes?","You have unsaved work, opening a new file will cause you to lose changes.\r\n\r\nContinue?"))
                {
                    return;
                }
            }

            var fileName = await Popups.ShowOpenFileAsync("Package Library (*.pckgs)|*.pckgs");
            if(!String.IsNullOrEmpty(fileName))
            {
                Packages = await _packageLibrary.GetPackagesAsync(fileName);
            }
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
            SaveLibraryCommand.RaiseCanExecuteChanged();
        }

        public async void SaveLibrary()
        {
            if (String.IsNullOrEmpty(_fileName))
            {
                _fileName = await Popups.ShowSaveFileAsync("Package Library (*.pckgs) | *.pckgs");
                if(String.IsNullOrEmpty(_fileName))
                {
                    return;
                }
            }

            await _packageLibrary.SavePackagesAsync(Packages,_fileName);

            _isDirty = false;
            SaveLibraryCommand.RaiseCanExecuteChanged();
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
        public RelayCommand OpenLibraryCommand { get; private set; }
        public RelayCommand SaveLibraryCommand { get; private set; }

        public RelayCommand SavePackageCommand { get; private set; }
        public RelayCommand DeletePackageCommand { get; private set; }
        public RelayCommand CancelPackageCommand { get; private set; }
    }
}
