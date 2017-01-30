

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MachineControlViewModel
    {

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
    }
}
