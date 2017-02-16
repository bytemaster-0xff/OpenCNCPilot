using HelixToolkit.Wpf;
using LagoVista.GCode.Sender.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LagoVista.GCode.Sender.Application.Converters
{
    public class HeightMap2MeshConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var map = value as HeightMap;

            var mb = new MeshBuilder(false, true);

            double Hdelta = map.MaxHeight - map.MinHeight;

            for (int x = 0; x < map.SizeX - 1; x++)
            {
                for (int y = 0; y < map.SizeY - 1; y++)
                {
                    if (!map.Points[x, y].HasValue || !map.Points[x, y + 1].HasValue || !map.Points[x + 1, y].HasValue || !map.Points[x + 1, y + 1].HasValue)
                        continue;

                    mb.AddQuad(
                        new System.Windows.Media.Media3D.Point3D(map.Min.X + (x + 1) * map.Delta.X / (map.SizeX - 1), map.Min.Y + (y) * map.Delta.Y / (map.SizeY - 1), map.Points[x + 1, y].Value),
                        new System.Windows.Media.Media3D.Point3D(map.Min.X + (x + 1) * map.Delta.X / (map.SizeX - 1), map.Min.Y + (y + 1) * map.Delta.Y / (map.SizeY - 1), map.Points[x + 1, y + 1].Value),
                        new System.Windows.Media.Media3D.Point3D(map.Min.X + (x) * map.Delta.X / (map.SizeX - 1), map.Min.Y + (y + 1) * map.Delta.Y / (map.SizeY - 1), map.Points[x, y + 1].Value),
                        new System.Windows.Media.Media3D.Point3D(map.Min.X + (x) * map.Delta.X / (map.SizeX - 1), map.Min.Y + (y) * map.Delta.Y / (map.SizeY - 1), map.Points[x, y].Value),
                        new System.Windows.Point(0, System.Convert.ToInt32((map.Points[x + 1, y].Value - map.MinHeight) * Hdelta)),
                        new System.Windows.Point(0, System.Convert.ToInt32((map.Points[x + 1, y + 1].Value - map.MinHeight) * Hdelta)),
                        new System.Windows.Point(0, System.Convert.ToInt32((map.Points[x, y + 1].Value - map.MinHeight) * Hdelta)),
                        new System.Windows.Point(0, System.Convert.ToInt32((map.Points[x, y].Value - map.MinHeight) * Hdelta))
                        );
                }
            }

            return mb.ToMesh();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
