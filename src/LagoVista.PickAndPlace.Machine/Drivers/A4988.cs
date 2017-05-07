using LagoVista.Core.Models;
using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace LagoViata.PNP.Drivers
{
    
    public class A4988 : ModelBase, IA4988
    {
        private int _axis;
        private int _dirPinNumber;

        AppService _appService;

        GpioPin _stepPin;
        GpioPin _dirPin;

        /// <summary>
        /// Last micro seconds when update was ran or most recent one if not busy.
        /// </summary>
        long _lastMicroSeconds;

        /// <summary>
        /// When the update loop should run again if we are busy.
        /// </summary>
        long _nextMicroSeconds;

        /// <summary>
        /// Calculated microseconds for pausing.
        /// </summary>
        long _pauseMicroSeconds;

        private int _stepsRemaining;
        private int _stepsRequested;
        private GpioPinValue _previousValue = GpioPinValue.Low;

        public A4988(int axis, int dirPin)
        {
            _axis = axis;
            _dirPinNumber = dirPin;
        }

        public void Init(GpioController gpioController, AppService appService)
        {
            _appService = appService;
            _dirPin = gpioController.OpenPin(_dirPinNumber);
        }
        
        public double PercentComplete
        {
            get; private set;
        }

        private bool _isBusy = false;
        
        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(ref _isBusy, value); }
        }

        public void Start(int steps, double feedRate, Direction direction)
        {
            _stepsRemaining = steps;
            _stepsRequested = steps;
            _dirPin.Write(direction == Direction.Backwards ? GpioPinValue.Low : GpioPinValue.High);
            IsBusy = true;

            _pauseMicroSeconds = 200;
            _nextMicroSeconds = _lastMicroSeconds;

            Debug.WriteLine($"Running {steps}");
        }

        public void Kill()
        {
            IsBusy = false;
            _stepsRemaining = 0;
        }

        public void Update(long uSeconds)
        {

            return;
            if (!IsBusy)
            {
                //Just grab the value so we can use it the next time we want to move.
                _lastMicroSeconds = uSeconds;
            }
            else
            {
                if (uSeconds > _nextMicroSeconds)
                {
                    if (_previousValue == GpioPinValue.Low)
                    {
                        _stepPin.Write(GpioPinValue.High);
                        _previousValue = GpioPinValue.High;
                    }
                    else
                    {
                        _stepPin.Write(GpioPinValue.Low);
                        _stepsRemaining--;
                        _previousValue = GpioPinValue.Low;
                    }

                    if (_stepsRemaining == 0)
                    {
                        Debug.WriteLine($"Finsihed!");
                        IsBusy = false;
                    }

                    _nextMicroSeconds = uSeconds + _pauseMicroSeconds;
                }
            }
        }
    }
}
