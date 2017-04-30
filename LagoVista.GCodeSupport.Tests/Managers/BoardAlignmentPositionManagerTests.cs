using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.GCode.Sender.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using LagoVista.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCodeSupport.Tests.Managers
{
    [TestClass]
    public class BoardAlignmentPositionManagerTests
    {

        Moq.Mock<IPCBManager> _pcbManager;
        BoardAlignmentPositionManager _mgr;



        [TestInitialize]
        public void Init()
        {
            _pcbManager = new Moq.Mock<IPCBManager>();
            _mgr = new BoardAlignmentPositionManager(_pcbManager.Object);
        }

        private void SetupFiducials(double x1, double y1, double x2, double y2)
        {
            _pcbManager.Setup(pcb => pcb.FirstFiducial).Returns(new Point2D<double> { X = x1, Y = y1 });
            _pcbManager.Setup(pcb => pcb.SecondFiducial).Returns(new Point2D<double> { X = x2, Y = y2 }); /* Difference is 30 */
        }

        [TestMethod()]
        public void PerfectAlignmentTest()
        {
            SetupFiducials(5, 5, 45, 45);

            _mgr.FirstLocated = new Point2D<double>(5, 5);
            _mgr.SecondLocated = new Point2D<double>(45, 45);

         
            Assert.IsTrue(_mgr.HasCalculatedOffset);
            Assert.AreEqual(0, _mgr.RotationOffset, 0.1);
            Assert.AreEqual(-5, _mgr.OffsetPoint.X, 0.1);
            Assert.AreEqual(-5, _mgr.OffsetPoint.Y, 0.1);
        }

        [TestMethod()]
        public void FortyFiveCCWAlignmentTest()
        {
            SetupFiducials(5, 5, 45, 45);

            Debug.WriteLine(new Point2D<double>(45, 45).Rotate(-45).X);
            Debug.WriteLine(new Point2D<double>(45, 45).Rotate(-45).Y);

            _mgr.FirstLocated = new Point2D<double>(5, 5);
            _mgr.SecondLocated = new Point2D<double>(0, 63);

         
            Assert.IsTrue(_mgr.HasCalculatedOffset);
            Assert.AreEqual(0, _mgr.RotationOffset.ToDegrees(), 0.1);
            Assert.AreEqual(-5, _mgr.OffsetPoint.X, 0.1);
            Assert.AreEqual(-5, _mgr.OffsetPoint.Y, 0.1);
        }
    }
}
