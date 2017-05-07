using LagoViata.PNP.Drivers;
using LagoVista.Core.Models;
using Windows.Devices.Gpio;

namespace LagoViata.PNP.Services
{
    public class Axis : ModelBase, IAxis
    {
        A4988 _stepper;
        EndStop _minEndStop;
        EndStop _maxEndStop;
        bool _isHoming = false;
        Direction _currentDirection;

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(ref _isBusy, value); }
        }

        public Axis(int stepPin, int dirPin)
        {
            _stepper = new A4988(stepPin, dirPin);
        }

        public void Init(GpioController gpioController)
        {
            _stepper.Init(gpioController);

            if(_minEndStop != null)
            {
                _minEndStop.Init(gpioController);
            }

            if(_maxEndStop != null)
            {
                _maxEndStop.Init(gpioController);
            }
        }
        
        public Axis(int stepPin, int dirPin, int minEndStopPin) : this(stepPin, dirPin)
        {
            _minEndStop = new EndStop(minEndStopPin);
        }

        public Axis(int stepPin, int dirPin, int minEndStopPin, int maxEndstopPin) : this(stepPin, dirPin, minEndStopPin)
        {
            _maxEndStop = new EndStop(maxEndstopPin);
        }

        public bool HasMaxEndStop { get { return _maxEndStop != null; } }
        public bool HasMinEndStop { get { return _maxEndStop != null; } }
    
        
        public bool MaxEndStopTrigger { get { return HasMaxEndStop ? _maxEndStop.Triggered : false; } }

        public bool MinEndStopTrigger{ get { return HasMinEndStop ? _minEndStop.Triggered : false; } }

        private double _currentLocation;
        public double CurrentLocation
        {
            get { return _currentLocation; }
            private set { Set(ref _currentLocation, value); }
        }

        private double _workOffset;
        public double WorkOffset
        {
            get { return _workOffset; }
            private set { Set(ref _workOffset, value); }
        }

        public void Home()
        {
            
        }

        public void Move(double newLocation, double feedRate)
        {
            _stepper.Start(System.Convert.ToInt32(300 * newLocation), 1000, feedRate > 0 ? Direction.Forward : Direction.Backwards);
        }

        public void Update(long uSeconds)
        {
            _stepper.Update(uSeconds);
            if (HasMaxEndStop) _maxEndStop.Update(uSeconds);
            if (HasMinEndStop) _minEndStop.Update(uSeconds);
        }

        public void Kill()
        {
            _stepper.Kill();
        }
    }
}
