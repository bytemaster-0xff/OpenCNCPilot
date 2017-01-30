using LagoVista.Core.ViewModels;
using System;

namespace LagoVista.GCode.Sender.ViewModels
{
    public class GCodeAppViewModel : ViewModelBase
    {
        private IMachine _machine;
        private Settings _settings;

        public GCodeAppViewModel(IMachine machine, Settings settings)
        {
            _settings = settings;
            _machine = machine;
        }

        public IMachine Machine { get { return _machine; } }
        public Settings Settings { get { return _settings; } }

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
