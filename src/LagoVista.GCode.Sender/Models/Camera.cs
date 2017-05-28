using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Models
{
    public class Camera : ModelBase
    {
        public String Id { get; set; }
        public int CameraIndex { get; set; }
        public String Name { get; set; }

        public Point2D<double> AbsolutePosition { get; set; }

        private Point2D<double> _tool1Offset;
        public Point2D<double> Tool1Offset
        {
            get { return _tool1Offset; }
            set { Set(ref _tool1Offset, value);  }
        }


        public Point2D<double> Tool2Offset { get; set; }
        public Point2D<double> Tool3Offset { get; set; }
    }
}
