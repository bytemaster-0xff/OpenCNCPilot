using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public class MVHomingViewModel : MachineVisionViewModelBase
    {
        public MVHomingViewModel(IMachine machine) : base(machine)
        {
            EndStopHomingCycleCommand = new RelayCommand(EndStopHomingCycle, () => HasFrame);
            BeginMVHomingCycleCommand = new RelayCommand(BeginMVHomingCycle, () => HasFrame);
            SetXYZeroCommand = new RelayCommand(SetXYZero, () => HasFrame);
        }

        protected override void CaptureStarted()
        {
            base.CaptureStarted();
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

        public void BeginMVHomingCycle()
        {
            
        }

        public void SetXYZero()
        {

        }        

        public void MVHomingCycle()
        {
            Machine.GotoPoint(Machine.Settings.HomeFiducialOffset, true);
        }

        public override async Task InitAsync()
        {
            await base.InitAsync();
        }

        public override void CircleLocated(Point2D<double> point, double diameter)
        {
            Machine.BoardAlignmentManager.CircleLocated(point);
        }

        public override void CornerLocated(Point2D<double> point)
        {
            Machine.BoardAlignmentManager.CornerLocated(point);
        }

        double _xZeroOffset;
        public double XZeroOffset
        {
            get { return _xZeroOffset; }
            set { Set(ref _xZeroOffset, value); }
        }

        double _yZeroOffset;
        public double YZeroOffset
        {
            get { return _yZeroOffset; }
            set { Set(ref _yZeroOffset, value); }
        }

        public RelayCommand EndStopHomingCycleCommand { get; private set; }
        public RelayCommand BeginMVHomingCycleCommand { get; private set; }

        public RelayCommand SetXYZeroCommand { get; private set; }
    }
}
