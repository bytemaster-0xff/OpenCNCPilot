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
            HomeCommand = new RelayCommand((param) => Home((HomeAxis)param), CanHome);
            CycleStartCommand = new RelayCommand(CycleStart, CanCycleStart);
            SetViewTypeCommand = new RelayCommand((param) => SetViewType((ViewTypes)param), CanSetViewType);

            Machine.PropertyChanged += Machine_PropertyChanged;
        }

        private void Machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.Connected) ||
               e.PropertyName == nameof(Machine.Mode))
            {
                SoftResetCommand.RaiseCanExecuteChanged();
                ClearAlarmCommand.RaiseCanExecuteChanged();
                FeedHoldCommand.RaiseCanExecuteChanged();
                CycleStartCommand.RaiseCanExecuteChanged(); 
                JogCommand.RaiseCanExecuteChanged();
                ResetCommand.RaiseCanExecuteChanged();
                HomeCommand.RaiseCanExecuteChanged();
            }

            if (e.PropertyName == nameof(Machine.Settings))
            {
                /* Keep the saved values as temp vars since updating the StepMode will overwrite */
                var originalXYStepSize = Machine.Settings.XYStepSize;
                var originalZStepSize = Machine.Settings.ZStepSize;

                XYStepMode = Machine.Settings.XYStepMode;
                ZStepMode = Machine.Settings.ZStepMode;

                XYStepSizeSlider = originalXYStepSize;
                ZStepSizeSlider = originalZStepSize;
            }
        }

        public void SetViewType(ViewTypes viewType)
        {
            Machine.ViewType = viewType;
        }

        public bool CanSetViewType(object param)
        {
            return Machine.Connected && Machine.Mode == OperatingMode.Manual;
        }

        public bool CanHome(object param)
        {
            return Machine.Connected && Machine.Mode == OperatingMode.Manual;
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
            return Machine.Connected && Machine.Mode == OperatingMode.Alarm;
        }

        public RelayCommand SetViewTypeCommand { get; private set; }
        public RelayCommand JogCommand { get; private set; }
        public RelayCommand HomeCommand { get; private set; }
        public RelayCommand ResetCommand { get; private set; }
        public RelayCommand SoftResetCommand { get; private set; }
        public RelayCommand ClearAlarmCommand { get; private set; }
        public RelayCommand FeedHoldCommand { get; private set; }
        public RelayCommand CycleStartCommand { get; private set; }
    }
}
