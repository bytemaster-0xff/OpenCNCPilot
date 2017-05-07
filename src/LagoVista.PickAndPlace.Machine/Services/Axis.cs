using LagoViata.PNP.Drivers;
using LagoVista.Core.Models;
using System;
using Windows.Devices.Gpio;

namespace LagoViata.PNP.Services
{
    public class Axis : ModelBase, IAxis
    {
        A4988 _stepper;
        EndStop _minEndStop;
        EndStop _maxEndStop;
        bool _isHoming = false;
        AppService _appService;
        double _destination;

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

        public void Init(GpioController gpioController, AppService appService)
        {
            _appService = appService;
            _stepper.Init(gpioController, appService);

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

        public void  Completed()
        {
            IsBusy = false;
            _currentLocation = _destination;
        }
    
        
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
            IsBusy = true;
            _destination = newLocation;

            var deltaLocation = newLocation - CurrentLocation;
            var direction = (deltaLocation > 0) ? Direction.Forward : Direction.Backwards;
            var steps = Convert.ToInt32(Math.Abs(deltaLocation) * 300);

            _stepper.Start(steps, 1000, direction);
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
