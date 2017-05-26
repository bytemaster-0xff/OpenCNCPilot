using LagoViata.PNP.Drivers;
using LagoVista.Core.Models;
using System;
using System.Diagnostics;
using Windows.Devices.Gpio;

namespace LagoViata.PNP.Services
{
    public class Axis : ModelBase, IAxis
    {
        EndStop _minEndStop;
        EndStop _maxEndStop;
        bool _isHoming = false;
        AppService _appService;
        double _destination;
        int _dirPinNumber;
        GpioPin _dirPin;
        int _axisNumber;

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(ref _isBusy, value); }
        }

        public Axis(int axisNumber, int dirPin)
        {
            _axisNumber = axisNumber;
            _dirPinNumber = dirPin;
        }


        public Axis(int axisNumber, int dirPin, int minEndStopPin) : this(axisNumber, dirPin)
        {
            _minEndStop = new EndStop(minEndStopPin);
        }

        public Axis(int axisNumber, int dirPin, int minEndStopPin, int maxEndstopPin) : this(axisNumber, dirPin, minEndStopPin)
        {
            _maxEndStop = new EndStop(maxEndstopPin);
        }

        public void Init(GpioController gpioController, AppService appService)
        {
            _appService = appService;
            _dirPin = gpioController.OpenPin(_dirPinNumber);

            if (_minEndStop != null)
            {
                _minEndStop.Init(gpioController);
            }

            if(_maxEndStop != null)
            {
                _maxEndStop.Init(gpioController);
            }
        }
        

        public bool HasMaxEndStop { get { return _maxEndStop != null; } }
        public bool HasMinEndStop { get { return _minEndStop != null; } }

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

        public async void Move(double newLocation, double feedRate)
        {
            if (newLocation != CurrentLocation)
            {
                IsBusy = true;
                _destination = newLocation;

                var deltaLocation = newLocation - CurrentLocation;
                var direction = (deltaLocation > 0) ? Direction.Forward : Direction.Backwards;
                var steps = Convert.ToInt32(Math.Abs(deltaLocation) * 300);

                Debug.WriteLine($"Sending {steps} on axis {_axisNumber}");
                _dirPin.Write(direction == Direction.Backwards ? GpioPinValue.Low : GpioPinValue.High);

                await _appService.StartStep(_axisNumber, steps, 2);
            }
        }

        public void Update(long ticks)
        {
            if (HasMaxEndStop) _maxEndStop.Update();
            if (HasMinEndStop) _minEndStop.Update();
        }

        public void Kill()
        {
            IsBusy = false;
        }
    }
}
