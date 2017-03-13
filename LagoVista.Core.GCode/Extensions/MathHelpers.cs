using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Core
{
    public static class MathHelpers
    {
        public static double ToDegrees(this double radians)
        {
            return radians * (180 / Math.PI);
        }

        public static double ToRadians(this double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        public static string ToDim(this double value)
        {
            return value.ToString("0.0000", new NumberFormatInfo() { NumberDecimalSeparator = "." });
        }

    }
}
