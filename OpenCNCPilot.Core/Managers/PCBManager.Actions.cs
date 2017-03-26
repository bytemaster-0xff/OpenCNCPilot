using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class PCBManager
    {

        public Point2D<double> GetAdjustedPoint(Point2D<double> point)
        {
            if(!HasMeasuredOffset)
            {
                return point;
            }
            else
            {
                //TDODO: REALLY NEED to create overloaded operators for Point2D
                var offsetPoint = new Point2D<double>(MeasuredOffset.X + point.X, MeasuredOffset.Y + point.Y);
                var rotatedPoint = offsetPoint.Rotate(MeasuredOffsetAngle);

                return rotatedPoint;
            }

        }
    }
}
