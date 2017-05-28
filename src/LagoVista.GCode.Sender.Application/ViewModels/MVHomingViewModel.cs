using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Diagnostics;
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

            SetXYZeroCommand = new RelayCommand(SetXYZero, () => HasFrame);
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
            SetXYZeroCommand.RaiseCanExecuteChanged();
        }

        protected override void CaptureEnded()
        {
            EndStopHomingCycleCommand.RaiseCanExecuteChanged();
            BeginMVHomingCycleCommand.RaiseCanExecuteChanged();
            SetXYZeroCommand.RaiseCanExecuteChanged();
        }

        public void EndStopHomingCycle()
        {
            Machine.HomingCycle();
        }

        public async void BeginMVHomingCycle()
        {
            _state = States.MVHoming;
            Machine.GotoPoint(Machine.Settings.HomeFiducialOffset, true);
            await Machine.MachineRepo.SaveAsync();
        }

        public void SetXYZero()
        {
            _state = States.Idle;
            Machine.SendCommand("G92 X Y");
        }

        public override async Task InitAsync()
        {
            await base.InitAsync();
        }

        public override void CircleCentered(Point2D<double> point, double diameter, Point2D<double> stdDeviation)
        {
        }

        int stabilizedPointCount = 0;

        public override void CircleLocated(Point2D<double> offset, double diameter, Point2D<double> stdDeviation)
        {
            if (_state == States.MVHoming)
            {
                if (stdDeviation.X < 0.5 && stdDeviation.Y < 0.5)
                {

                    stabilizedPointCount++;
                    if (stabilizedPointCount > 5)
                    {
                        var newLocationX = Math.Round(Machine.MachinePosition.X - (offset.X / 20), 4);
                        var newLocationY = Math.Round(Machine.MachinePosition.Y - (offset.Y / 20), 4);
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

        public override void CornerLocated(Point2D<double> point, Point2D<double> stdDev)
        {

            Machine.BoardAlignmentManager.CornerLocated(point);
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

        public RelayCommand SetXYZeroCommand { get; private set; }
    }
}
