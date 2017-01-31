using LagoVista.Core.Commanding;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class HeightMapViewModel
    {
        private void InitCommanding()
        {
            PauseProbingCommand = new RelayCommand(PauseProbing, CanPauseProbing);
            StartProbingCommand = new RelayCommand(StartProbing, CanStartProbing);

            Machine.ProbeFinished += ProbingFinished;
            Machine.PropertyChanged += _machine_PropertyChanged;
        }


        private void _machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.Mode))
            {
                StartProbingCommand.RaiseCanExecuteChanged();
                PauseProbingCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanStartProbing()
        {
            return Machine.Mode == OperatingMode.Manual && _currentHeightMap != null;
        }

        public bool CanPauseProbing()
        {
            return Machine.Mode == OperatingMode.ProbingHeightMap;
        }


        public RelayCommand PauseProbingCommand { get; private set; }

        public RelayCommand StartProbingCommand { get; private set; }
    }
}
