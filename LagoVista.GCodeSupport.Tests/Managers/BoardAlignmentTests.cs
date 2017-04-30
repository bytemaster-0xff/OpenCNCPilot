using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.GCode.Sender.Managers;
using LagoVista.GCodeSupport.Tests.Mocks;
using LagoVista.Core.Models.Drawing;
using Moq;
using LagoVista.Core;

namespace LagoVista.GCodeSupport.Tests.Managers
{
    [TestClass]
    public class BoardAlignmentTests
    {
        Moq.Mock<IMachine> _machine;
        Moq.Mock<IPCBManager> _pcbManager;
        Moq.Mock<IPointStabilizationFilter> _pointStabilizationFilter;
        IBoardAlignmentManager _boardAlignmentManager;

        [TestInitialize]
        public void Init()

        {
            _machine = new Moq.Mock<IMachine>();
            _pcbManager = new Moq.Mock<IPCBManager>();
            _pointStabilizationFilter = new Moq.Mock<IPointStabilizationFilter>();
            _boardAlignmentManager = new BoardAlignmentManager(_machine.Object, new FakeLogger(),  _pcbManager.Object, _pointStabilizationFilter.Object);
        }


        [TestCleanup]
        public void Cleanup()
        {
            _boardAlignmentManager.Dispose();
        }

        private void SetupFiducials(double x1, double y1, double x2, double y2)
        {
            _pcbManager.Setup(pcb => pcb.FirstFiducial).Returns(new Point2D<double> { X = x1, Y = y1 });
            _pcbManager.Setup(pcb => pcb.SecondFiducial).Returns(new Point2D<double> { X = x2, Y = y2 }); /* Difference is 30 */
        }

        private void SetMachineLocation(double x1, double y1)
        {
            _machine.Setup(mach => mach.MachinePosition).Returns(new Core.Models.Drawing.Vector3(x1, y1, 0));
            _boardAlignmentManager.SetNewMachineLocation(new Point2D<double>(x1, y1));
        }

        public bool HasPointStabilized
        {
            set
            {
                _pointStabilizationFilter.Setup(pf => pf.HasStabilizedPoint).Returns(value);
            }
        }

        public Point2D<double> StabilizedPoint
        {
            set
            {
                _pointStabilizationFilter.Setup(pf => pf.StabilizedPoint).Returns(value);
            }
        }

        Point2D<double> _machineLocation;

        public Point2D<double> MachineLocation
        {
            get { return _machineLocation; }
            set
            {
                _machineLocation = value;
                SetMachineLocation(value.X, value.Y);
            }
        }

        public void CircleCaptured(Point2D<double> point)
        {
            _boardAlignmentManager.CircleLocated(point);
        }

        [TestMethod]
        public void ShouldChangeStateToMoveToSecondFiducialAfterFirstFiducialLocated()
        {
            SetupFiducials(5, 5, 35, 35);
            HasPointStabilized = true;
            MachineLocation = new Point2D<double>(10, 10);
            StabilizedPoint = new Point2D<double>(0.5, 0.5);
            _boardAlignmentManager.State = BoardAlignmentManagerStates.StabilzingAfterFirstFiducialMove;

            CircleCaptured(new Point2D<double>(5, 5));

            Assert.AreEqual(BoardAlignmentManagerStates.MovingToSecondFiducial, _boardAlignmentManager.State);
        }


        [TestMethod]
        public void ShouldSendGCodeToJogToSecondExpectedFiducialLocation()
        {
            SetupFiducials(5, 5, 35, 35);
            HasPointStabilized = true;
            MachineLocation = new Point2D<double>(10, 10);
            StabilizedPoint = new Point2D<double>(0.5, 0.5);
            _boardAlignmentManager.State = BoardAlignmentManagerStates.StabilzingAfterFirstFiducialMove;

            _machine.Setup(mac => mac.SendCommand(It.IsAny<string>()));

            CircleCaptured(new Point2D<double>(5, 5));

            var secondFiducialX = MachineLocation.X + (_pcbManager.Object.SecondFiducial.X - _pcbManager.Object.FirstFiducial.X);
            var secondFiducialY = MachineLocation.Y + (_pcbManager.Object.SecondFiducial.Y - _pcbManager.Object.FirstFiducial.Y);

            _machine.Verify(mac => mac.SendCommand($"G0 X{secondFiducialX.ToDim()} Y{secondFiducialY.ToDim()}"),Moq.Times.Once);
        }
    }
}
