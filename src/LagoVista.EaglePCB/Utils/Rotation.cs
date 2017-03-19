using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.EaglePCB
{
    public static class RotationExtension
    {
        public static Point2D<double> Rotate(this Point2D<double> point, Point2D<double> origin, double angle)
        {
            var radians = (Math.PI / 180) * (angle);

            var cos = Math.Cos(radians);
            var sin = Math.Sin(radians);
            
            var rotX = (cos * (point.X - origin.X)) - (sin * (point.Y - origin.Y));
            var rotY = (cos * (point.Y - origin.Y)) + (sin * (point.X - origin.X));
            
            return new Point2D<double>(rotX, rotY);
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
            if(double.TryParse(angleStr, out angle))
            {
                if (rotationString.Contains("L"))
                    return 360 - angle;

                return angle;
            }

            return angle;
        }
    }
}
