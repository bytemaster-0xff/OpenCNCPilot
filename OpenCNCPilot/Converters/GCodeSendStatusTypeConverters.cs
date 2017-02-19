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
    public class GCodeSendBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var statusType = (Core.GCode.Commands.GCodeCommand.StatusTypes)value;
            switch(statusType)
            {
                case Core.GCode.Commands.GCodeCommand.StatusTypes.Ready: return "White";
                case Core.GCode.Commands.GCodeCommand.StatusTypes.Queued: return "LightGray";
            }

            return "White";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GCodeSendForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var statusType = (Core.GCode.Commands.GCodeCommand.StatusTypes)value;

            switch (statusType)
            {
                case Core.GCode.Commands.GCodeCommand.StatusTypes.Ready: return "DarkGray";
                case Core.GCode.Commands.GCodeCommand.StatusTypes.Queued: return "Black";
                case Core.GCode.Commands.GCodeCommand.StatusTypes.Sent: return "Blue";
                case Core.GCode.Commands.GCodeCommand.StatusTypes.Acknowledged: return "Green";
            }

            return "Black";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
