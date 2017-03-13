using System.Globalization;

namespace LagoVista.EaglePCB
{
    public static class ToDimExtension
    {        
        public static string ToDim(this double value)
        {
            return value.ToString("0.0000", new NumberFormatInfo() { NumberDecimalSeparator = "." });
        }
    }

}
