using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LagoVista.GCode.Sender.Application.Converters
{
    public class MessageTypeBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var messageType = (StatusMessageTypes)value;
            if (messageType == StatusMessageTypes.Warning)
                return "Blue";
            
            return "White";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageTypeForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var messageType = (StatusMessageTypes)value;

            switch(messageType)
            {
                case StatusMessageTypes.FatalError: return "Red";
                case StatusMessageTypes.ReceivedLine:
                case StatusMessageTypes.SentLine: return "Green";
                case StatusMessageTypes.SentLinePriority: return "Red";
                case StatusMessageTypes.Warning: return "Yellow";
            }

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
