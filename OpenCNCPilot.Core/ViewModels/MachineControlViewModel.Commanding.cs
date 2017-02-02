using LagoVista.Core.Commanding;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MachineControlViewModel
    {
        private void InitCommands()
        {
            JogCommand = new RelayCommand((param) => Jog((JogDirections)param), CanJog);
            ResetCommand = new RelayCommand((param) => ResetAxis((ResetAxis)param), CanResetAxis);

            SoftResetCommand = new RelayCommand(SoftReset, CanSoftReset);
            ClearAlarmCommand = new RelayCommand(ClearAlarm, CanClearAlarm);
            FeedHoldCommand = new RelayCommand(FeedHold, CanFeedHold);
            CycleStartCommand = new RelayCommand(CycleStart, CanCycleStart);

            Machine.PropertyChanged += Machine_PropertyChanged;
        }

        private void Machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(Machine.Connected) ||
               e.PropertyName == nameof(Machine.Mode))
            {
                SoftResetCommand.RaiseCanExecuteChanged();
                ClearAlarmCommand.RaiseCanExecuteChanged();
                FeedHoldCommand.RaiseCanExecuteChanged();
                CycleStartCommand.RaiseCanExecuteChanged();
                JogCommand.RaiseCanExecuteChanged();
                ResetCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanResetAxis(object param)
        {
            return Machine.Connected && Machine.Mode == OperatingMode.Manual;
        }

        public bool CanJog(object param)
        {
            return Machine.Connected && Machine.Mode == OperatingMode.Manual;
        }

        public bool CanCycleStart()
        {
            return Machine.Connected;
        }

        public bool CanFeedHold()
        {
            return Machine.Connected;
        }

        public bool CanSoftReset()
        {
            return Machine.Connected;
        }

        public bool CanClearAlarm()
        {
            return Machine.Connected && Machine.Status.ToLower().StartsWith("alarm");
        }

        public RelayCommand JogCommand { get; private set; }
        public RelayCommand ResetCommand { get; private set; }
        public RelayCommand SoftResetCommand { get; private set; }
        public RelayCommand ClearAlarmCommand { get; private set; }
        public RelayCommand FeedHoldCommand { get; private set; }
        public RelayCommand CycleStartCommand { get; private set; }
    }
}
