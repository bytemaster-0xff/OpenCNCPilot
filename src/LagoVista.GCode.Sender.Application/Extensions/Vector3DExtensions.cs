
using System.Windows.Media.Media3D;

namespace LagoVista.GCode.Sender.Application
{
    public static class Vector3DExtensions
    {
        public static Point3D ToMedia3D(this LagoVista.Core.Models.Drawing.Point3D<double> point3d)
        {
            return new Point3D(point3d.X, point3d.Y, point3d.Z);
        }
    }
}
