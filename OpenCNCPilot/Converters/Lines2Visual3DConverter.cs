using LagoVista.GCode.Sender.Managers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace LagoVista.GCode.Sender.Application.Converters
{
    public class Lines2Visual3DConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            /* No lines, OK, don't render anything */
            if(value == null)
            {
                return null;
            }

            var inputPoints = value as ObservableCollection<Line3D>;
            if (inputPoints == null)
            {
                /* If not a list of points...bug, so throw exception */
                throw new Exception("Invalid Convesion Point, must provide ObservableCollection of Line3D");
            }

            var outputPoints = new Point3DCollection();
            foreach (var inputPoint in inputPoints)
            {
                outputPoints.Add(new Point3D() { X = inputPoint.Start.X, Y = inputPoint.Start.Y, Z = inputPoint.Start.Z });
                outputPoints.Add(new Point3D() { X = inputPoint.End.X, Y = inputPoint.End.Y, Z = inputPoint.End.Z });
            }
            return outputPoints;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
