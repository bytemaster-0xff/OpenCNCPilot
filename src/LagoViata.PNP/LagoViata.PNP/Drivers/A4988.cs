using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace LagoViata.PNP.Drivers
{
    public enum Direction
    {
        Forward,
        Backwards
    }

    public class A4988
    {
        private int _stepPinNumber;
        private int _dirPinNumber;
        
        GpioPin _stepPin;
        GpioPin _dirPin;
        

        public A4988(int stepPin, int dirPin)
        {
            _stepPinNumber = stepPin;
            _dirPinNumber = dirPin;

        }

        public async Task Init()
        {
            if (LightningProvider.IsLightningEnabled)
            {
                var gpioController = (await GpioController.GetControllersAsync(LightningGpioProvider.GetGpioProvider()))[0];

                _stepPin = gpioController.OpenPin(_stepPinNumber);
                _dirPin = gpioController.OpenPin(_dirPinNumber);
            }        
        }

        public async Task Step(int steps, Direction direction )
        {
            await Task.Run(() =>
            {
                _dirPin.Write(direction == Direction.Backwards ? GpioPinValue.Low : GpioPinValue.High);

                var sw = new System.Diagnostics.Stopwatch();

                for (var idx = 0; idx < steps; ++idx)
                {
                    _stepPin.Write(GpioPinValue.High);
                    sw.Start(); while ((sw.Elapsed).TotalMilliseconds < 10) { }
                    sw.Reset();
                    _stepPin.Write(GpioPinValue.Low);
                    sw.Start(); while ((sw.Elapsed).TotalMilliseconds < 10) { }
                    sw.Reset();
                }
            });
        }
    }
}
