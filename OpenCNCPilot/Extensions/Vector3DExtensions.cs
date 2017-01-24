
using System.Windows.Media.Media3D;

namespace OpenCNCPilot
{
    public static class Vector3DExtensions
    {
        public static Point3D ToMedia3D(this OpenCNCPilot.Core.Util.Point3D point3d)
        {
            return new Point3D(point3d.X, point3d.Y, point3d.Z);
        }
    }
}
