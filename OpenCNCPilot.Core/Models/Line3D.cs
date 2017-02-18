using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Models
{
    public class Line3D
    {
        public LagoVista.Core.Models.Drawing.Vector3 Start { get; set; }
        public LagoVista.Core.Models.Drawing.Vector3 End { get; set; }

        public static Line3D Create(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            return new Line3D()
            {
                Start = new Core.Models.Drawing.Vector3(x1, y1, z1),
                End = new Core.Models.Drawing.Vector3(x2, y2, z2)
            };
        }
    }
}
