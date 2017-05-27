
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

        public bool _vacuumOn = false;
        public bool VacuumOn
        {
            get { return _vacuumOn; }
            set
            {
                Machine.SendCommand(value ? "M62 P1" : "M62 P0");
                _vacuumOn = value;
                RaisePropertyChanged();
            }
        }


        public bool _exhaustSolendoidOn = false;
        public bool ExhaustSolendoidOn
        {
            get { return _exhaustSolendoidOn; }
            set
            {
                Machine.SendCommand(value ? "M64 P1" : "M64 P0");
                _exhaustSolendoidOn = value;
                RaisePropertyChanged();
            }
        }


        public bool _suctionSolenoidOn = false;
        public bool SuctionSolenoidOn
        {
            get { return _suctionSolenoidOn; }
            set
            {
                Machine.SendCommand(value ? "M63 P1" : "M63 P0");
                _suctionSolenoidOn = value;
                RaisePropertyChanged();
            }
        }
    }
}
