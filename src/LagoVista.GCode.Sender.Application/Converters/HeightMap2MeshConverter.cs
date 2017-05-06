using HelixToolkit.Wpf;
using LagoVista.GCode.Sender.Models;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Linq;

namespace LagoVista.GCode.Sender.Application.Converters
{
    public class HeightMap2MeshConverter : IValueConverter
    {
        /// <summary>
        /// Returns a point based on it's X, Y index
        /// </summary>
        /// <param name="xIndex">X Index into list</param>
        /// <param name="yIndex">Y Index into list</param>
        /// <returns></returns>
        private HeightMapProbePoint GetPoint(ObservableCollection<HeightMapProbePoint> points, int xIndex, int yIndex)
        {
            return points.Where(pnt => pnt.XIndex == xIndex && pnt.YIndex == yIndex).FirstOrDefault();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var map = value as HeightMap;
            if (map == null)
            {
                throw new Exception("Invalid Height Map Type.");
            }

            var mb = new MeshBuilder(false, true);
            var delta = map.MaxHeight - map.MinHeight;

            for (var x = 0; x < map.SizeX - 1; x++)
            {
                for (var y = 0; y < map.SizeY - 1; y++)
                {
                    if (GetPoint(map.Points, x, y).Status != HeightMapProbePointStatus.Probed ||
                        GetPoint(map.Points, x, y + 1).Status != HeightMapProbePointStatus.Probed ||
                        GetPoint(map.Points, x + 1, y).Status != HeightMapProbePointStatus.Probed ||
                        GetPoint(map.Points, x + 1, y + 1).Status != HeightMapProbePointStatus.Probed)
                        continue;

                    mb.AddQuad(
                        new System.Windows.Media.Media3D.Point3D(map.Min.X + (x + 1) * map.Delta.X / (map.SizeX - 1), map.Min.Y + (y) * map.Delta.Y / (map.SizeY - 1), GetPoint(map.Points, x + 1, y).Point.Z),
                        new System.Windows.Media.Media3D.Point3D(map.Min.X + (x + 1) * map.Delta.X / (map.SizeX - 1), map.Min.Y + (y + 1) * map.Delta.Y / (map.SizeY - 1), GetPoint(map.Points, x + 1, y + 1).Point.Z),
                        new System.Windows.Media.Media3D.Point3D(map.Min.X + (x) * map.Delta.X / (map.SizeX - 1), map.Min.Y + (y + 1) * map.Delta.Y / (map.SizeY - 1), GetPoint(map.Points, x, y + 1).Point.Z),
                        new System.Windows.Media.Media3D.Point3D(map.Min.X + (x) * map.Delta.X / (map.SizeX - 1), map.Min.Y + (y) * map.Delta.Y / (map.SizeY - 1), GetPoint(map.Points, x, y).Point.Z),
                        new System.Windows.Point(0, System.Convert.ToInt32((GetPoint(map.Points, x + 1, y).Point.Z - map.MinHeight) * delta)),
                        new System.Windows.Point(0, System.Convert.ToInt32((GetPoint(map.Points, x + 1, y + 1).Point.Z - map.MinHeight) * delta)),
                        new System.Windows.Point(0, System.Convert.ToInt32((GetPoint(map.Points, x, y + 1).Point.Z - map.MinHeight) * delta)),
                        new System.Windows.Point(0, System.Convert.ToInt32((GetPoint(map.Points, x, y).Point.Z - map.MinHeight) * delta))
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
