using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Models
{
    public enum HeightMapProbePointStatus
    {
        NotProbed,
        Probed
    }
    public class HeightMapProbePoint
    {
        public int XIndex { get; set; }
        public int YIndex { get; set; }
        public LagoVista.Core.Models.Drawing.Vector3 Point { get; set; }

        public HeightMapProbePointStatus Status { get; set; }
    }
}
