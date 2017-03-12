using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.EaglePCB
{
    public static class ToDimExtension
    {
        public static NumberFormatInfo DecimalOutputFormat
        {
            get
            {
                return new NumberFormatInfo() { NumberDecimalSeparator = "." };
            }
        }

        public static string ToDim(this double value)
        {
            return value.ToString("0.0000", DecimalOutputFormat);
        }
    }

}
