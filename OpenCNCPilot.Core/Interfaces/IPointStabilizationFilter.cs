using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Interfaces
{
    public interface IPointStabilizationFilter
    {
        void Add(Point2D<double> cameraOffsetPixels);

        bool HasStabilizedPoint { get; }

        Point2D<double> StabilizedPoint { get; }
    }
}
