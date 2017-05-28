using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LagoVista.GCode.Sender.Application.Converters
{
    class Point2TextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return "-";
            }

            var pt = value as Point2D<double>;

            return $"({Math.Round(pt.X,4)} - {Math.Round(pt.Y,4)})";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
