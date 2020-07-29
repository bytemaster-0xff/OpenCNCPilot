using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public class MVHomingViewModel : MachineVisionViewModelBase
    {
        public enum States
        {
            Idle,
            Homing,
            MVHoming,
        }

        public MVHomingViewModel(IMachine machine) : base(machine)
        {
            EndStopHomingCycleCommand = new RelayCommand(EndStopHomingCycle, () => HasFrame);
            BeginMVHomingCycleCommand = new RelayCommand(BeginMVHomingCycle, () => HasFrame);
            
            GoToFiducialHomeCommand = new RelayCommand(GoToFiducialHome, () => HasFrame);

            CalibrateFiducialHomeCommand = new RelayCommand(CalibrateFiducialHome, () => HasFrame);

            SetFiducialHomeCommand = new RelayCommand(SetFiducialHome, () => HasFrame);
            if (Machine.Settings.HomeFiducialOffset == null)
            {
                Machine.Settings.HomeFiducialOffset = new Point2D<double>(5, 5);
            }
        }

        States _state = States.Idle;

        public bool CanSetZeroOffset()
        {
            return XZeroOffset.HasValue && YZeroOffset.HasValue;
        }

        protected override void CaptureStarted()
        {
            EndStopHomingCycleCommand.RaiseCanExecuteChanged();
            BeginMVHomingCycleCommand.RaiseCanExecuteChanged();
            SetFiducialHomeCommand.RaiseCanExecuteChanged();
            GoToFiducialHomeCommand.RaiseCanExecuteChanged();
            CalibrateFiducialHomeCommand.RaiseCanExecuteChanged();
        }

        protected override void CaptureEnded()
        {
            EndStopHomingCycleCommand.RaiseCanExecuteChanged();
            BeginMVHomingCycleCommand.RaiseCanExecuteChanged();
            SetFiducialHomeCommand.RaiseCanExecuteChanged();
            GoToFiducialHomeCommand.RaiseCanExecuteChanged();
            CalibrateFiducialHomeCommand.RaiseCanExecuteChanged();
        }

        public override async Task InitAsync()
        {
            await base.InitAsync();
            Machine.TopLightOn = true;
            ShowTopCamera = true;
            ShowCircles = false;
            ShowHarrisCorners = true;
            StartCapture();
        }

        public void EndStopHomingCycle()
        {
            Machine.HomingCycle();
        }

        public async void BeginMVHomingCycle()
        {
            Machine.PCBManager.Tool1Navigation = true;
            _state = States.MVHoming;
            Machine.GotoPoint(Machine.Settings.HomeFiducialOffset, true);
            await Machine.MachineRepo.SaveAsync();
        }

        public void CalibrateFiducialHome()
        {            
            Machine.SendCommand("M80");
            _state = States.Idle;
        }

        public void GoToFiducialHome()
        {
            _state = States.Idle;
            Machine.SendCommand("M53");
        }

        public void SetFiducialHome()
        {
            _state = States.Idle;
            Machine.SendCommand("M70");
        }

        public override void CircleCentered(Point2D<double> point, double diameter)
        {
            XZeroOffset = Machine.MachinePosition.X;
            YZeroOffset = Machine.MachinePosition.Y;
        }

        int stabilizedPointCount = 0;

        public override void CircleLocated(Point2D<double> offset, double diameter, Point2D<double> stdDeviation)
        {
            if (_state == States.MVHoming)
            {
                if (stdDeviation.X < 0.5 && stdDeviation.Y < 0.5)
                {
                    stabilizedPointCount++;
                    if (stabilizedPointCount > 10)
                    {
                        var newLocationX = -Math.Round((offset.X / 20), 4) + Machine.MachinePosition.X;
                        var newLocationY = Math.Round((offset.Y / 20), 4) + Machine.MachinePosition.Y;
                        Machine.GotoPoint(new Point2D<double>() { X = newLocationX, Y = newLocationY }, true);
                        stabilizedPointCount = 0;
                        XZeroOffset = newLocationX;
                        YZeroOffset = newLocationY;
                    }
                }
                else
                {
                    XZeroOffset = null;
                    YZeroOffset = null;
                    stabilizedPointCount = 0;
                }
            }
        }

        public override void CornerLocated(Point2D<double> offset, Point2D<double> stdDeviation)
        {
            Machine.BoardAlignmentManager.CornerLocated(offset);

            if (_state == States.MVHoming)
            {
                if (stdDeviation.X < 0.5 && stdDeviation.Y < 0.5)
                {
                    stabilizedPointCount++;
                    if (stabilizedPointCount > 10)
                    {
                        var newLocationX = -Math.Round((offset.X / 20), 4) + Machine.MachinePosition.X;
                        var newLocationY = Math.Round((offset.Y / 20), 4) + Machine.MachinePosition.Y;
                        Machine.GotoPoint(new Point2D<double>() { X = newLocationX, Y = newLocationY }, true);
                        stabilizedPointCount = 0;
                        XZeroOffset = newLocationX;
                        YZeroOffset = newLocationY;
                    }
                }
                else
                {
                    XZeroOffset = null;
                    YZeroOffset = null;
                    stabilizedPointCount = 0;
                }
            }
        }

        double? _xZeroOffset;
        public double? XZeroOffset
        {
            get { return _xZeroOffset; }
            set { Set(ref _xZeroOffset, value); }
        }

        double? _yZeroOffset;
        public double? YZeroOffset
        {
            get { return _yZeroOffset; }
            set { Set(ref _yZeroOffset, value); }
        }

        public RelayCommand EndStopHomingCycleCommand { get; private set; }
        public RelayCommand BeginMVHomingCycleCommand { get; private set; }

        public RelayCommand SetFiducialHomeCommand { get; private set; }

        public RelayCommand GoToFiducialHomeCommand { get; private set; }

        public RelayCommand CalibrateFiducialHomeCommand { get; private set; }
    }
}
