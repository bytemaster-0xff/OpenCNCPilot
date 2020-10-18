using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace LagoVista.GCode.Sender.Application.Converters
{
    public class BoolColorConverter : IValueConverter
    {
        private static SolidColorBrush _redBrush = new SolidColorBrush(Colors.Red);
        private static SolidColorBrush _greenBrush = new SolidColorBrush(Colors.Green);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is Boolean && (bool)value) ? _redBrush : _greenBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
