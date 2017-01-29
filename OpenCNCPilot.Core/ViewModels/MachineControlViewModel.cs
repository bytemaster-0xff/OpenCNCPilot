using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;

namespace LagoVista.GCode.Sender.ViewModels
{
    public class MachineControlViewModel : ViewModelBase
    {
        IMachine _machine;

        public enum JogDirections
        {
            YPlus,
            YMinus,
            XPlus,
            XMinus,
            ZPlus,
            ZMinus
        }

        public enum Axis
        {
            XY,
            Z
        }

        public enum Reset
        {
            X,
            Y,
            Z,
            All
        }

        public enum MicroStepModes
        {
            Micro,
            Small,
            Medium,
            Large
        }

        public MachineControlViewModel(IMachine machine, Settings settings)
        {
            _machine = machine;

            XYStep = settings.MediumStepSize;
            ZStep = settings.MediumStepSize;

            //JogCommand = new RelayCommand((param) => Jog((JogDirections)param), (param) => { return _machine.Connected; });
            JogCommand = new RelayCommand((param) => Jog((JogDirections)param), CanJog);
            ResetCommand = new RelayCommand((param) => ResetAxis((Reset)param), CanResetAxis);

            SoftResetCommand = new RelayCommand(SoftReset, CanSoftReset);
            ClearAlarmCommand = new RelayCommand(ClearAlarm, CanClearAlarm);
            FeedHoldCommand = new RelayCommand(FeedHold, CanFeedHold);
            CycleStartCommand = new RelayCommand(CycleStart, CanCycleStart);

            SetMicroStepSizeCommand = new RelayCommand((param) => SetStepSize((Axis)param, MicroStepModes.Micro));
        }

        public void SetStepSize(Axis axis, MicroStepModes stepSize)
        {
            double maxStep = 0;
            double minStep = 0;

            switch (stepSize)
            {
                case MicroStepModes.Large:
                    maxStep = 20;
                    minStep = 1;
                    break;
                case MicroStepModes.Medium:
                    maxStep = 10;
                    minStep = 1;
                    break;
                case MicroStepModes.Small:
                    maxStep = 1;
                    minStep = 0.1;
                    break;
                case MicroStepModes.Micro:
                    maxStep = 0.1;
                    minStep = 0.01;
                    break;
            }


            if (axis == Axis.XY)
            {
                XYStepMin = minStep;
                XYStepMax = maxStep;
                XYStep = (minStep + maxStep) / 2;
            }
            else
            {
                ZStepMin = minStep;
                ZStepMax = maxStep;
                ZStep = (minStep + maxStep) / 2;
            }
        }

        public bool CanResetAxis(object param)
        {
            return _machine.Connected && _machine.Mode == OperatingMode.Manual;
        }

        public bool CanJog(object param)
        {
            return _machine.Connected && _machine.Mode == OperatingMode.Manual;
        }

        public void CycleStart()
        {
            _machine.CycleStart();
        }

        public bool CanCycleStart()
        {
            return _machine.Connected;
        }


        public void SoftReset()
        {
            _machine.SoftReset();
        }

        public bool CanFeedHold()
        {
            return _machine.Connected;
        }

        public void FeedHold()
        {
            _machine.FeedHold();
        }

        public bool CanSoftReset()
        {
            return _machine.Connected;
        }

        public bool CanClearAlarm()
        {
            return _machine.Connected && _machine.Status.ToLower().StartsWith("alarm");
        }

        public void ClearAlarm()
        {
            _machine.ClearAlarm();
        }


        public void Jog(JogDirections direction)
        {
/*            switch (direction)
            {

            }*/
        }

        public void ResetAxis(Reset axis)
        {
            /*var command  = $"G10 L2 P0 X{_machine.WorkPosition.X.ToString(Constants.DecimalOutputFormat)} Y{_machine.WorkPosition.Y.ToString(Constants.DecimalOutputFormat)} Z{_machine.WorkPosition.Z.ToString(Constants.DecimalOutputFormat)}";
            var cmd2  = "G92 X0 Y0 Z0";
            var cmd3  = "G10 L2 P0 X0 Y0 Z0";*/
        }

        private double _xyStep;
        public double XYStep
        {
            get { return _xyStep; }
            set { Set(ref _xyStep, value); }
        }

        private double _zStep;
        public double ZStep
        {
            get { return _zStep; }
            set { Set(ref _zStep, value); }
        }

        private double _xyStepMin;
        public double XYStepMin
        {
            get { return _xyStepMin; }
            set { Set(ref _xyStepMin, value); }
        }

        private double _xyStepMax;
        public double XYStepMax
        {
            get { return _xyStepMax; }
            set { Set(ref _xyStepMax, value); }
        }

        private double _zStepMin;
        public double ZStepMin
        {
            get { return _zStepMin; }
            set { Set(ref _zStepMin, value); }
        }

        private double _zStepMax;
        public double ZStepMax
        {
            get { return _zStepMax; }
            set { Set(ref _zStepMax, value); }
        }

        public RelayCommand JogCommand { get; private set; }
        public RelayCommand ResetCommand { get; private set; }

        public RelayCommand SetMicroStepSizeCommand { get; private set; }
     
        public IMachine Machine { get { return _machine; } }

        public RelayCommand SoftResetCommand { get; private set; }

        public RelayCommand ClearAlarmCommand { get; private set; }

        public RelayCommand FeedHoldCommand { get; private set; }

        public RelayCommand CycleStartCommand { get; private set; }
    }
}
