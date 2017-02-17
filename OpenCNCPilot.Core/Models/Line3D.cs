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
    }
}
