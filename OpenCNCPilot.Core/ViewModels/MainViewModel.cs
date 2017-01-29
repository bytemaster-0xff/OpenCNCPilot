using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using System;

namespace LagoVista.GCode.Sender.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        IMachine _machine;

        public MainViewModel(IMachine machine)
        {
            _machine = machine;
            OpenHeightMapCommand = new RelayCommand(OpenHeightMapFile, CanPerformFileOperation);
            OpenGCodeFileCommand = new RelayCommand(OpenGCodeFile, CanPerformFileOperation);
            CloseFileCommand = new RelayCommand(CloseFile, CanPerformFileOperation);

            _machine.PropertyChanged += _machine_PropertyChanged;
        }

        private void _machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(_machine.Mode))
            {
                DispatcherServices.Invoke(() =>
                {
                    OpenHeightMapCommand.RaiseCanExecuteChanged();
                    OpenGCodeFileCommand.RaiseCanExecuteChanged();
                    CloseFileCommand.RaiseCanExecuteChanged();
                });
            }
        }

        private bool CanPerformFileOperation(Object instance)
        {
            return (_machine.Mode != OperatingMode.SendingJob);
        }

        public async void OpenGCodeFile(object instance)
        {
            var file = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!String.IsNullOrEmpty(file))
            {
                var allLines = await Storage.ReadAllLinesAsync(file);
                _machine.SetFile(allLines);
            }
        }

        public async void OpenHeightMapFile(object instance)
        {
            var file = await Popups.ShowOpenFileAsync(Constants.FileFilterHeightMap);
        }

        public void CloseFile(object instance)
        {
            _machine.ClearFile();
        }

        public RelayCommand OpenHeightMapCommand { get; private set; }
        public RelayCommand OpenGCodeFileCommand { get; private set; }
        public RelayCommand CloseFileCommand { get; private set; }
    }
}
