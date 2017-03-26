using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Util
{
    public class PointStabilizationFilter : IPointStabilizationFilter
    {
        List<Point2D<double>> _stablizationList;
        Point2D<double> _stabilizedPoint;
        private double _pixelEpsilon;
        private int _inToleranceCount;
        public const int IN_TOLERANCE_COUNT_REQUIRED = 10;

        /* This is the maximum error in pixels that will be allowed to determine that we have 
         * found the fiducial */
        private const double EPSILON_FIDUCIAL_PIXELS = 2.0;

        public PointStabilizationFilter(double pixelEpsilon, int inTolernaceCount)
        {
            _inToleranceCount = inTolernaceCount;
            _pixelEpsilon = pixelEpsilon;
            _stablizationList = new List<Point2D<double>>();
        }

        public void Add(Point2D<double> cameraOffsetPixels)
        {
            _stabilizedPoint = null;

            if (_stablizationList.Any())
            {
                var avgX = _stablizationList.Average(pt => pt.X);
                var avgY = _stablizationList.Average(pt => pt.Y);

                var deltaX = Math.Abs(cameraOffsetPixels.X) - Math.Abs(avgX);
                var deltaY = Math.Abs(cameraOffsetPixels.Y) - Math.Abs(avgY);

                /* If the current one coming in is not within the range of the average clear the list,
                 * values coming in are already filtered. */
                if (Math.Abs(deltaX) > _pixelEpsilon || Math.Abs(deltaY) > _pixelEpsilon)
                {
                    _stablizationList.Clear();
                }

                if (_stablizationList.Count < _inToleranceCount)
                {
                    _stablizationList.Add(new Point2D<double>(cameraOffsetPixels.X, cameraOffsetPixels.Y));
                }

                if (_stablizationList.Count >= _inToleranceCount)
                {
                    _stabilizedPoint = new Point2D<double>(avgX, avgY);
                }
            }
            else
            {
                _stablizationList.Add(new Point2D<double>(cameraOffsetPixels.X, cameraOffsetPixels.Y));
            }
        }

        public void Reset()
        {
            _stablizationList.Clear();
        }

        public bool HasStabilizedPoint
        {
            get
            {
                return _stabilizedPoint != null;
            }
        }

        public Point2D<double> StabilizedPoint
        {
            get { return _stabilizedPoint; }
        }

        public int PointCount
        {
            get { return _stablizationList.Count; }
        }
    }
}
