using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LagoVista.GCode.Sender.Util
{
    /* Not sure how we use generics here, can't really constrain Point<T> to a numeric value 
     * at least that I know of */
    public class FloatMedianFilter
    {
        int _filterSize = 12;
        int _throwAwaySize = 3;

        public Point2D<float>[] _points;
        int _head;
        int _arraySize;

        public FloatMedianFilter(int size = 12, int throwAway = 3)
        {
            _filterSize = size;
            _throwAwaySize = throwAway;
            _points = new Point2D<float>[size];
        }

        public void Add(Point2D<float> point)
        {
            _points[_head++] = point;
            if (_head == _filterSize)
            {
                _head = 0;
            }

            _arraySize++;
            _arraySize = Math.Min(_filterSize, _arraySize);
        }

        public void Add(float x, float y)
        {
            Add(new Point2D<float>(x, y));
        }

        public Point2D<double> StandardDeviation
        {
            get
            {
                var nonNullPoints = _points.Where(pt => pt != null);

                if (nonNullPoints.Count() == _filterSize / 2)
                    return new Point2D<double>(999.9, 999.9);

                
                var avgX = nonNullPoints.Select(pt => pt.X).Average();
                var avgY = nonNullPoints.Select(pt => pt.Y).Average();

                var sigmaDeltaX = 0.0;
                var sigmaDeltaY = 0.0;

                foreach (var point in nonNullPoints)
                {
                    sigmaDeltaX += Math.Pow(point.X - avgX, 2);
                    sigmaDeltaY += Math.Pow(point.Y - avgY, 2);
                }

                return new Point2D<double>()
                {
                    X = Math.Round(Math.Sqrt(sigmaDeltaX / nonNullPoints.Count()), 2),
                    Y = Math.Round(Math.Sqrt(sigmaDeltaY / nonNullPoints.Count()), 2),
                };
            }
        }

        public Point2D<double> Filtered
        {
            get
            {
                var nonNullPoints = _points.Where(pt => pt != null);
                if (nonNullPoints.Count() < 5)
                    return null;

                var sortedX = nonNullPoints.Select(pt => pt.X).OrderBy(pt => pt);
                var sortedY = nonNullPoints.Select(pt => pt.Y).OrderBy(pt => pt);

                if (sortedX.Count() == _filterSize)
                {
                    var subsetX = sortedX.Skip(_throwAwaySize).Take(_filterSize - _throwAwaySize * 2);
                    var subsetY = sortedY.Skip(_throwAwaySize).Take(_filterSize - _throwAwaySize * 2);
                    return new Point2D<double>(Math.Round(subsetX.Average(), 4), Math.Round(subsetY.Average(), 4));
                }
                else
                {
                    var avgX = nonNullPoints.Select(pt => pt.X).Average();
                    var avgY = nonNullPoints.Select(pt => pt.Y).Average();
                    return new Point2D<double>(Math.Round(avgX, 4), Math.Round(avgY, 4));
                }
            }
        }
    }

    /* Not sure how we use generics here, can't really constrain Point<T> to a numieric value 
     * at least that I know of */
    public class IntMedianFilter
    {
        int _filterSize = 12;
        int _throwAwaySize = 3;

        public Point2D<int>[] _points;
        int _head;
        int _arraySize;

        public IntMedianFilter(int size = 12, int throwAway = 3)
        {
            _filterSize = size;
            _throwAwaySize = throwAway;
            _points = new Point2D<int>[size];
        }

        public void Add(int x, int y)
        {
            Add(new Point2D<int>(x, y));
        }

        public void Add(Point2D<int> point)
        {
            _points[_head++] = point;
            if (_head == _filterSize)
            {
                _head = 0;
            }

            _arraySize++;
            _arraySize = Math.Min(_filterSize, _arraySize);
        }

        public Point2D<double> Filtered
        {
            get
            {
                var sortedX = _points.Where(pt => pt != null).Select(pt => pt.X).OrderBy(pt => pt);
                var sortedY = _points.Where(pt => pt != null).Select(pt => pt.Y).OrderBy(pt => pt);

                if (sortedX.Count() == 0)
                    return null;

                if (sortedX.Count() == _filterSize)
                {
                    var subsetX = sortedX.Skip(_throwAwaySize).Take(_filterSize - _throwAwaySize * 2);
                    var subsetY = sortedY.Skip(_throwAwaySize).Take(_filterSize - _throwAwaySize * 2);
                    return new Point2D<double>(subsetX.Average(), subsetY.Average());
                }
                else
                {
                    return new Point2D<double>(sortedX.Average(), sortedY.Average());
                }
            }
        }
    }
}