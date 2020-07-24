using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Core.GCode
{
    public enum ParseDistanceMode
    {
        Absolute,
        Relative
    }

    public enum ParseUnit
    {
        Metric,
        Imperial
    }

    public enum ArcPlane
    {
        XY = 0,
        YZ = 1,
        ZX = 2
    }

    public enum ArcDirection
    {
        CW,
        CCW
    }
}
