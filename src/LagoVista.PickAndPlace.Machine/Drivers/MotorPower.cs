using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace LagoViata.PNP.Drivers
{
    public class MotorPower
    {
        private int _enablePinNumber;
        GpioPin _enablePin;


        public MotorPower(int pin)
        {
            _enablePinNumber = pin;
        }

        public void Init(GpioController gpioController)
        {
            _enablePin = gpioController.OpenPin(_enablePinNumber);
        }

        public void Enable()
        {
            _enablePin.Write(GpioPinValue.Low);
        }

        public void Disable()
        {
            _enablePin.Write(GpioPinValue.High);
        }
    }
}
