using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace LagoViata.PNP.Drivers
{
    public class Accessory
    {
        int _pinNumber;
        GpioPin _pin;

        public Accessory(int pinNumber)
        {
            _pinNumber = pinNumber;
        }

        public void Init(GpioController gpioController)
        {
            _pin = gpioController.OpenPin(_pinNumber);
            _pin.SetDriveMode(GpioPinDriveMode.Output);
        }

        public void On()
        {
            _pin.Write(GpioPinValue.High);
        }

        public void Off()
        {
            _pin.Write(GpioPinValue.Low);
        }

    }
}
