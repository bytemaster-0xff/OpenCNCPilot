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
            SetCameraCommand = new RelayCommand((param) => SetCamera(), CanSetCamera);
            SetTool1Command = new RelayCommand((param) => SetTool1(), CanSetTool1);

            MoveToBottomCameraCommand = new RelayCommand((obj) => MoveToBottomCamera(), CanJog);

            SetToMoveHeightCommand = new RelayCommand((obj) => SetToMoveHeight(), CanJog);
            SetToPickHeightCommand = new RelayCommand((obj) => SetToPickHeight(), CanJog);
            SetToPlaceHeightCommand = new RelayCommand((obj) => SetToBoardHeight(), CanJog);

            Machine.PropertyChanged += Machine_PropertyChanged;
        }

        void SetToMoveHeight()
        {
            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M54");
            }
            else
            {
                Machine.SendCommand($"G0 Z{Machine.Settings.ToolSafeMoveHeight} F5000");
            }
        }

        void SetToPickHeight()
        {
            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M55");
            }
            else
            {
                Machine.SendCommand($"G0 Z{Machine.Settings.ToolPickHeight} F5000");
            }
        }

        void SetToBoardHeight()
        {
            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M56");
            }
            else
            {
                Machine.SendCommand($"G0 Z{Machine.Settings.ToolBoardHeight} F5000");
            }
        }

        void MoveToBottomCamera()
        {
            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M52");
            }
            else
            {
                if (Machine.Settings.PartInspectionCamera?.AbsolutePosition != null)
                {
                    Machine.SendCommand($"G0 X{Machine.Settings.PartInspectionCamera.AbsolutePosition.X} Y{Machine.Settings.PartInspectionCamera.AbsolutePosition.Y} Z{Machine.Settings.PartInspectionCamera.FocusHeight} F1{Machine.Settings.FastFeedRate}");
                }
            }
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
                SetCameraCommand.RaiseCanExecuteChanged();
                SetTool1Command.RaiseCanExecuteChanged();

                SetToMoveHeightCommand.RaiseCanExecuteChanged();
                SetToPickHeightCommand.RaiseCanExecuteChanged();
                SetToPlaceHeightCommand.RaiseCanExecuteChanged();
                MoveToBottomCameraCommand.RaiseCanExecuteChanged();
            }

            if(e.PropertyName == nameof(Machine.ViewType))
            {
                SetCameraCommand.RaiseCanExecuteChanged();
                SetTool1Command.RaiseCanExecuteChanged();
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

        public bool CanSetCamera(object param)
        {
            return Machine.ViewType == ViewTypes.Tool1;
        }

        public bool CanSetTool1(object param)
        {
            return Machine.ViewType == ViewTypes.Camera;
        }

        public void SetCamera()
        {
            Machine.ViewType = ViewTypes.Camera;
        }

        public void SetTool1()
        {
            Machine.ViewType = ViewTypes.Tool1;
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

        public RelayCommand JogCommand { get; private set; }
        public RelayCommand HomeCommand { get; private set; }
        public RelayCommand ResetCommand { get; private set; }
        public RelayCommand SoftResetCommand { get; private set; }
        public RelayCommand ClearAlarmCommand { get; private set; }
        public RelayCommand FeedHoldCommand { get; private set; }
        public RelayCommand CycleStartCommand { get; private set; }

        public RelayCommand SetToMoveHeightCommand { get; private set; }
        public RelayCommand SetToPickHeightCommand { get; private set; }
        public RelayCommand SetToPlaceHeightCommand { get; private set; }
        public RelayCommand MoveToBottomCameraCommand { get; private set; }

        public RelayCommand SetCameraCommand { get; private set; }
        public RelayCommand SetTool1Command { get; private set; }
    }
}
