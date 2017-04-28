
namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class JobControlViewModel
    {
        public bool IsCreatingHeightMap { get { return Machine.Mode == OperatingMode.ProbingHeightMap; } }
        public bool IsProbingHeight { get { return Machine.Mode == OperatingMode.ProbingHeight; } }
        public bool IsRunningJob { get { return Machine.Mode == OperatingMode.SendingGCodeFile; } }

        private bool _topLightOn = false;
        public bool TopLightOn
        {
            get { return _topLightOn; }
            set
            {
                Machine.SendCommand(value ? "M60 P1" : "M60 P0");
                _topLightOn = value;
                RaisePropertyChanged();
            }
        }

        private bool _bottomLightOn = false;
        public bool BottomLightOn
        {
            get { return _bottomLightOn; }
            set
            {
                Machine.SendCommand(value ? "M61 P1" : "M61 P0");
                _bottomLightOn = value;
                RaisePropertyChanged();
            }
        }
    }
}
