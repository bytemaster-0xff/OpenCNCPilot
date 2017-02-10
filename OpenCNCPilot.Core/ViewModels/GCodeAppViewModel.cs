using LagoVista.Core.ViewModels;
using LagoVista.GCode.Sender.Interfaces;
using System;

namespace LagoVista.GCode.Sender.ViewModels
{
    public class GCodeAppViewModel : ViewModelBase
    {
        private IMachine _machine;

        public GCodeAppViewModel(IMachine machine)
        {
            _machine = machine;
        }

        public GCodeAppViewModel()
        {

        }

        public IMachine Machine { get { return _machine; } set { _machine = value; } }

        public void AssertInManualMode(Action action)
        {
            if (Machine.Mode != OperatingMode.Manual)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Machine Busy");
            }
            else
            {
                action();
            }            
        }
    }
}
