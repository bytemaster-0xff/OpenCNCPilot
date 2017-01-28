using OpenCNCPilot.Core.GCode;
using OpenCNCPilot.Core.GCode.GCodeCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCNCPilot.Core.Communication
{
    public partial class Machine
    {
        private ParseDistanceMode _distanceMode = ParseDistanceMode.Absolute;
        public ParseDistanceMode DistanceMode
        {
            get { return _distanceMode; }
            private set
            {
                if (_distanceMode == value)
                    return;
                _distanceMode = value;

                RaiseEvent(DistanceModeChanged);
            }
        }

        private ParseUnit _unit = ParseUnit.Metric;
        public ParseUnit Unit
        {
            get { return _unit; }
            private set
            {
                if (_unit == value)
                    return;
                _unit = value;

                RaiseEvent(UnitChanged);
            }
        }

        private ArcPlane _plane = ArcPlane.XY;
        public ArcPlane Plane
        {
            get { return _plane; }
            private set
            {
                if (_plane == value)
                    return;
                _plane = value;

                RaiseEvent(PlaneChanged);
            }
        }

    }
}
