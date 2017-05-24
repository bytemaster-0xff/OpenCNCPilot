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
                return "X:-, Y:-;";
            }

            var pt = value as Point2D<double>;

            return $"X:{pt.X}, Y:{pt.Y};";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
