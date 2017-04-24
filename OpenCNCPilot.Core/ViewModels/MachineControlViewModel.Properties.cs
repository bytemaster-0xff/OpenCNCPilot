

using System.Collections.Generic;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MachineControlViewModel
    {
        private double _xyStepMin;
        public double XYStepMin
        {
            get { return _xyStepMin; }
            set { Set(ref _xyStepMin, value); }
        }

        private double _xyStepInterval;
        public double XYStepInterval
        {
            get { return _xyStepInterval; }
            set { Set(ref _xyStepInterval, value); }
        }

        private double _xyStepMax;
        public double XYStepMax
        {
            get { return _xyStepMax; }
            set { Set(ref _xyStepMax, value); }
        }

        public double XYStepSize
        {
            get { return Machine.Settings.XYStepSize; }
            set
            {
                Machine.Settings.XYStepSize = value;
                RaisePropertyChanged();
            }
        }

        private double _xySizeStepSlider;
        public double XYStepSizeSlider
        {
            get { return _xySizeStepSlider; }
            set
            {
                _xySizeStepSlider = value;
                RaisePropertyChanged();

                var newValue = System.Convert.ToInt32(value / XYStepInterval) * XYStepInterval;
                var fractional = value % XYStepInterval;
                if (fractional > XYStepInterval / 2)
                    newValue += XYStepInterval;

                XYStepSize = newValue;
            }
        }


        private double _zStepInterval;
        public double ZStepInterval
        {
            get { return _zStepInterval; }
            set { Set(ref _zStepInterval, value); }
        }

        private double _zStepMax;
        public double ZStepMax
        {
            get { return _zStepMax; }
            set { Set(ref _zStepMax, value); }
        }

        private double _zStepMin;
        public double ZStepMin
        {
            get { return _zStepMin; }
            set { Set(ref _zStepMin, value); }
        }

        public double ZStepSize
        {
            get { return Machine.Settings.ZStepSize; }
            set
            {
                Machine.Settings.ZStepSize = value;
                RaisePropertyChanged();
            }
        }

        private double _zSizeStepSlider;
        public double ZStepSizeSlider
        {
            get { return _zSizeStepSlider; }
            set
            {
                _zSizeStepSlider = value;
                RaisePropertyChanged();

                var newValue = System.Convert.ToInt32(value / ZStepInterval) * ZStepInterval;
           
                ZStepSize = newValue;
            }
        }

        public StepModes XYStepMode
        {
            get { return Machine.Settings.XYStepMode; }
            set
            {
                Machine.Settings.XYStepMode = value;
                RaisePropertyChanged();
                switch (XYStepMode)
                {
                    case StepModes.XLarge:
                        XYStepInterval = 5;
                        XYStepMax = 100;
                        XYStepMin = 20;
                        XYStepSize = 50;
                        XYStepSizeSlider = XYStepSize;
                        break;
                    case StepModes.Large:
                        XYStepInterval = 1;
                        XYStepMax = 20;
                        XYStepMin = 10;
                        XYStepSize = 10;
                        XYStepSizeSlider = XYStepSize;
                        break;
                    case StepModes.Medium:
                        XYStepInterval = 1;
                        XYStepMax = 10;
                        XYStepMin = 1;
                        XYStepSize = 1;
                        XYStepSizeSlider = XYStepSize;
                        break;
                    case StepModes.Small:
                        XYStepInterval = 0.1;
                        XYStepMax = 1;
                        XYStepMin = 0.1;
                        XYStepSize = 0.1;
                        XYStepSizeSlider = XYStepSize;
                        break;
                    case StepModes.Micro:
                        XYStepInterval = 0.01;
                        XYStepMax = 0.1;
                        XYStepMin = 0.01;
                        XYStepSize = 0.01;
                        XYStepSizeSlider = XYStepSize;
                        break;
                }
            }
        }

        public StepModes ZStepMode
        {
            get { return Machine.Settings.ZStepMode; }
            set
            {
                Machine.Settings.ZStepMode = value;
                RaisePropertyChanged();
                switch (ZStepMode)
                {
                    case StepModes.Large:
                        ZStepInterval = 1;
                        ZStepMax = 20;
                        ZStepMin = 10;
                        ZStepSize = 10;
                        ZStepSizeSlider = ZStepSize;
                        break;
                    case StepModes.Medium:
                        ZStepInterval = 1;
                        ZStepMax = 10;
                        ZStepMin = 1;
                        ZStepSize = 1;
                        ZStepSizeSlider = ZStepSize;
                        break;
                    case StepModes.Small:
                        ZStepInterval = 0.1;
                        ZStepMax = 1;
                        ZStepMin = 0.1;
                        ZStepSize = 0.1;
                        ZStepSizeSlider = ZStepSize;
                        break;
                    case StepModes.Micro:
                        ZStepInterval = 0.01;
                        ZStepMax = 0.1;
                        ZStepMin = 0.01;
                        ZStepSize = 0.01;
                        ZStepSizeSlider = ZStepSize;
                        break;
                }
            }
        }

        public List<KeyValuePair<StepModes, string>> StepSizesXY
        {
            get
            {
                return new List<KeyValuePair<StepModes, string>>()
                {
                    new KeyValuePair<StepModes, string>(StepModes.XLarge, "20 - 100"),
                    new KeyValuePair<StepModes, string>(StepModes.Large, "10 - 20"),
                    new KeyValuePair<StepModes, string>(StepModes.Medium, "1 - 10"),
                    new KeyValuePair<StepModes, string>(StepModes.Small, "0.1 - 1.0"),
                    new KeyValuePair<StepModes, string>(StepModes.Micro, "0.01 - 0.1"),
                };
            }
            set { }
        }

        public List<KeyValuePair<StepModes, string>> StepSizesZ
        {
            get
            {
                return new List<KeyValuePair<StepModes, string>>()
                {
                    new KeyValuePair<StepModes, string>(StepModes.Large, "10 - 20"),
                    new KeyValuePair<StepModes, string>(StepModes.Medium, "1 - 10"),
                    new KeyValuePair<StepModes, string>(StepModes.Small, "0.1 - 1.0"),
                    new KeyValuePair<StepModes, string>(StepModes.Micro, "0.01 - 0.1"),
                };
            }
            set { }
        }
    }
}
