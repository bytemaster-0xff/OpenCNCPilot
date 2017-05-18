using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista
{
    public static class RotationExtension
    {
        public static Point2D<double> Rotate(this Point2D<double> point, Point2D<double> origin, double angle)
        {
            var radians = (Math.PI / 180) * (angle);

            double x1 = point.X - origin.X;
            double y1 = point.Y - origin.Y;

            double x2 = x1 * Math.Cos(radians) - y1 * Math.Sin(radians);
            double y2 = x1 * Math.Sin(radians) + y1 * Math.Cos(radians);
            
            return new Point2D<double>(x2 + origin.X, y2 + origin.Y);
        }

        public static Point2D<double> Rotate(this Point2D<double> point, double angle)
        {
            return point.Rotate(new Point2D<double>(0, 0), angle);
        }

        public static double ToAngle(this string rotationString)
        {
            double angle = 0;

            if (String.IsNullOrEmpty(rotationString))
                return angle;

            //HATE THIS CODE...don't know the spec on Eagle, very likely a sleeping bug.
            //TODO: Find spec on eagle to get a better understanding.
            var startIndex = 1;
            /* Prettu sure this is telling me "M" = "Mirror" */
            if (rotationString.StartsWith("M"))
            {
                startIndex = 2;
            }

            var angleStr = rotationString.Substring(startIndex);
            if (double.TryParse(angleStr, out angle))
            {
                if (rotationString.Contains("L"))
                    return 360 - angle;

                return angle;
            }

            return angle;
        }
    }
}
