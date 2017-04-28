using LagoVista.GCode.Sender;
using LagoVista.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace
{
    public class PnPManager
    {
        private Machine _machine;

        private void RaisePropertyChanged()
        {

        }

        public PnPManager(Machine machine)
        {
            _machine = machine;
        }

        private bool _topCameraLight;
        public bool TopCameraLight
        {
            get { return _topCameraLight; }
            set
            {
                _topCameraLight = value;
                _machine.SendCommand(value ? "M60 P1" : "M60 P2");
                RaisePropertyChanged();
            }
        }

        private bool _bottomCameraLight;
        public bool BottomCameraLight
        {
            get { return _bottomCameraLight; }
            set
            {
                _bottomCameraLight = value;
                _machine.SendCommand(value ? "M61 P1" : "M60 P2");
                RaisePropertyChanged();
            }
        }

        private bool _vacuumPump;
        public bool VacuumPump
        {
            get { return _vacuumPump; }
            set
            {
                _vacuumPump = value;
                _machine.SendCommand(value ? "M61 P1" : "M60 P2");
                RaisePropertyChanged();
            }
        }

        private double _placeHeadHeight;
        public double PlaceHeadHight
        {
            get { return _placeHeadHeight; }
            set
            {
                _placeHeadHeight = value;
                if (_placeHeadHeight != value)
                {
                    _machine.SendCommand("T0");
                    _machine.SendCommand($"G0 Z{value.ToDim()}"); 
                    RaisePropertyChanged();
                }
            }
        }

        private double _pastHeadHeight;
        public double PastHeadHight
        {
            get { return _pastHeadHeight; }
            set
            {
                _pastHeadHeight = value;
                if (_pastHeadHeight != value)
                {
                    _machine.SendCommand("T1");
                    _machine.SendCommand($"G0 Z{value.ToDim()}");
                    RaisePropertyChanged();
                }
            }
        }

        public void RotatePart(double angle)
        {
            _machine.SendCommand($"G0 C{angle}");
        }
    }
}
