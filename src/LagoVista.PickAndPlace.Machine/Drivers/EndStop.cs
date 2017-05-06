using LagoVista.Core.Models;
using Windows.Devices.Gpio;

namespace LagoViata.PNP.Drivers
{
    public class EndStop : ModelBase
    {
        GpioPin _endStopPin;
        public int _pinNumber;
        public EndStop(int pinNumber)
        {
            _pinNumber = pinNumber;
        }

        public void Init(GpioController gpioController)
        {
            _endStopPin = gpioController.OpenPin(_pinNumber);
        }

        private bool _triggered = false;
        public bool Triggered
        {
            get { return _triggered; }
            set { Set(ref _triggered, value); }
        }

        public void Update(long uSeconds)
        {
            Triggered = _endStopPin.Read() == GpioPinValue.Low;
        }
    }
}
