using LagoVista.Core.Models;
using LagoVista.Core;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public class BoardAlignmentPositionManager : ModelBase, IBoardAlignmentPositionManager
    {
        IPCBManager _boardManager;

        public BoardAlignmentPositionManager(IPCBManager boardManager)
        {
            _boardManager = boardManager;
        }

        private void CalculateOffset()
        {
            var actualTheta = Math.Atan2(SecondLocated.Y - FirstLocated.Y, SecondLocated.X - FirstLocated.X);
            var expectedTheta = Math.Atan2(SecondExpected.Y - FirstLocated.Y, SecondExpected.X - FirstLocated.X);
            RotationOffset = expectedTheta - actualTheta;

            var deltaX = FirstLocated.X - _boardManager.FirstFiducial.X;
            var deltaY = FirstLocated.Y - _boardManager.FirstFiducial.Y;

            var initial = new Point2D<double>(deltaX, deltaY);
            OffsetPoint = initial.Rotate(FirstLocated, RotationOffset.ToDegrees());

            BoardOriginPoint = new Point2D<double>()
            {
                X = FirstLocated.X - deltaX,
                Y = FirstLocated.Y - deltaY
            };

            HasCalculatedOffset = true;
        }

        /// <summary>
        /// Reset all the values
        /// </summary>
        public void Reset()
        {
            HasCalculatedOffset = false;
            FirstLocated = null;
            SecondLocated = null;
            OffsetPoint = null;
            BoardOriginPoint = null;
            SecondExpected = null;
            RotationOffset = 0;
        }

        /// <summary>
        /// True if there is a calculated offset and false. if not
        /// </summary>
        private bool _hasCalculatedOffset;
        public bool HasCalculatedOffset
        {
            get { return _hasCalculatedOffset; }
            set
            {
                _hasCalculatedOffset = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Offset from Position of First Location
        /// </summary>
        private Point2D<double> _offsetPoint;
        public Point2D<double> OffsetPoint
        {
            get { return _offsetPoint; }
            set { Set(ref _offsetPoint, value); }
        }

        /// <summary>
        /// Absolute position with respect to current work where the board is located
        /// </summary>
        private Point2D<double> _boardOriginPoint;
        public Point2D<double> BoardOriginPoint
        {
            get { return _boardOriginPoint; }
            set { Set(ref _boardOriginPoint, value); }

        }

        /// <summary>
        /// Angle of the board with respect to 0 being "east", CCW is Positive
        /// </summary>
        private double _rotationOffset;
        public double RotationOffset
        {
            get { return _rotationOffset; }
            set { Set(ref _rotationOffset, value); }
        }

        /// <summary>
        /// Machine X,Y Position of the Located First Fiducial
        /// </summary>
        private Point2D<double> _firstLocated;
        public Point2D<double> FirstLocated
        {
            get { return _firstLocated; }
            set
            {
                Set(ref _firstLocated, value);
                SecondExpected = new Point2D<double>()
                {
                    X = value.X + (_boardManager.SecondFiducial.X - _boardManager.FirstFiducial.X),
                    Y = value.Y + (_boardManager.SecondFiducial.Y - _boardManager.FirstFiducial.Y),
                };
            }
        }

        /// <summary>
        /// Machine X, Y Position of the Second Located Fiducial
        /// </summary>
        private Point2D<double> _secondLocated;
        public Point2D<double> SecondLocated
        {
            get { return _secondLocated; }
            set
            {
                Set(ref _secondLocated, value);
                CalculateOffset();
            }
        }

        /// <summary>
        /// Position of where the second fiducial should be if the board is aligned perfectly, this is used
        /// to determine the angle of the baord to determine the exact offsets.
        /// </summary>
        private Point2D<double> _secondExpected;
        public Point2D<double> SecondExpected
        {
            get { return _secondExpected; }
            private set { Set(ref _secondExpected, value); }
        }
    }
}
