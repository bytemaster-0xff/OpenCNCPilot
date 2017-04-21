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
                if (Machine.Settings.PositioningCamera != null &&
                    Machine.Settings.PositioningCamera.Tool1Offset != null &&
                    Machine.PCBManager.JogMode == Interfaces.BoardJogModes.Camera)
                {
                    return new Point2D<double>()
                    {
                        X = point.X + Machine.Settings.PositioningCamera.Tool1Offset.X,
                        Y = point.Y+ Machine.Settings.PositioningCamera.Tool1Offset.Y,
                    };
                        
                }
                else
                {
                    return point;
                }
            }
            else
            {
                //TDODO: REALLY NEED to create overloaded operators for Point2D
                var offsetPoint = new Point2D<double>(MeasuredOffset.X + point.X, MeasuredOffset.Y + point.Y);
                var rotatedPoint = offsetPoint.Rotate(MeasuredOffsetAngle);

                if (Machine.Settings.PositioningCamera != null && 
                    Machine.Settings.PositioningCamera.Tool1Offset != null &&
                    Machine.PCBManager.JogMode == Interfaces.BoardJogModes.Camera)
                {
                    return new Point2D<double>()
                    {
                        X = rotatedPoint.X + Machine.Settings.PositioningCamera.Tool1Offset.X,
                        Y = rotatedPoint.Y + Machine.Settings.PositioningCamera.Tool1Offset.Y,
                    };
                }
                else
                {
                    return rotatedPoint;
                }
            }

        }
    }
}
