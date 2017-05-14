using LagoVista.Core.Models;
using System.Diagnostics;
using Windows.Devices.Gpio;

namespace LagoViata.PNP.Drivers
{
    public class EndStop : ModelBase
    {
        GpioPin _endStopPin;
        public int _pinNumber;
        bool _lastTriggered;

        public EndStop(int pinNumber)
        {
            _pinNumber = pinNumber;
        }

        public void Init(GpioController gpioController)
        {
            _endStopPin = gpioController.OpenPin(_pinNumber);
            _endStopPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
        }

        private bool _triggered = false;
        public bool Triggered
        {
            get { return _triggered; }
            set { Set(ref _triggered, value); }
        }
        
        public void Update()
        {
            var isTriggered = _endStopPin.Read() == GpioPinValue.Low;
            if(Triggered != isTriggered)
            {
                Debug.WriteLine($"TRIGGERED STATE CHANGED: {_pinNumber} {isTriggered}");
                Triggered = isTriggered;
            }

        }
    }
}
