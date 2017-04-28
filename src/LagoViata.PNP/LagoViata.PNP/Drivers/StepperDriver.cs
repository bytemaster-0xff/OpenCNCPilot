using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace LagoViata.PNP.Drivers
{
    public class StepperDriver
    {
        const int ENABLE_PIN = 26;

        A4988 _yaxis;
        A4988 _xaxis;
        A4988 _caxis;

        A4988 _tool0;
        A4988 _tool1;


        GpioPin _enablePin;

        public StepperDriver()
        {
            _yaxis = new A4988(21, 20);
            _xaxis = new A4988(19, 13);

            _caxis = new A4988(6, 5);
            _tool0 = new A4988(22, 27);
            _tool1 = new A4988(17, 4);
        }

        public async Task InitAsync()
        {
            await _yaxis.Init();

            if (LightningProvider.IsLightningEnabled)
            {
                var gpioController = (await GpioController.GetControllersAsync(LightningGpioProvider.GetGpioProvider()))[0];

                _enablePin = gpioController.OpenPin(ENABLE_PIN);
            }
        }

        public void Enable()
        {
            _enablePin.Write(GpioPinValue.Low);
        }

        public void Disable()
        {
            _enablePin.Write(GpioPinValue.High);
        }

        public A4988 YAxis
        {
            get { return _yaxis; }
        }
    }
}
