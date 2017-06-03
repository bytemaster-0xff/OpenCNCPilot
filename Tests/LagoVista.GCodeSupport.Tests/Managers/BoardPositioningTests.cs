using LagoVista.GCode.Sender.Interfaces;
using LagoVista.GCode.Sender.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LagoVista.Core;

namespace LagoVista.GCodeSupport.Tests.Managers
{
    [TestClass]
    public class BoardPositioningTests
    {
        Moq.Mock<IPCBManager> _pcbManager;
        BoardAlignmentPositionManager _mgr;

        [TestInitialize]
        public void FindOffset()
        {
            _pcbManager = new Moq.Mock<IPCBManager>();
            _pcbManager.Setup(fid1 => fid1.FirstFiducial).Returns(new Core.Models.Drawing.Point2D<double>(5, 5));
            _pcbManager.Setup(fid2 => fid2.SecondFiducial).Returns(new Core.Models.Drawing.Point2D<double>(25, 25));
            _mgr = new BoardAlignmentPositionManager(_pcbManager.Object);
        }

        [TestMethod]
        public void Calculte2ndExpected()
        {
            /* If board was perfectly aligned, based on location of first position, calculate where expected location should be */
            _mgr.FirstLocated = new Core.Models.Drawing.Point2D<double>(20, 20);
            Assert.AreEqual(40, _mgr.SecondExpected.Y);
            Assert.AreEqual(40, _mgr.SecondExpected.X);
        }


        /*https://www.desmos.com/calculator/kohoey3j60 */

        [TestMethod]
        public void FindOffset_PerfectlyLevelBoard()
        {
            _mgr.FirstLocated = new Core.Models.Drawing.Point2D<double>(20, 20);
            _mgr.SecondLocated = _mgr.SecondExpected;
           
            Assert.AreEqual(0, _mgr.RotationOffset);

            Assert.AreEqual(15, _mgr.OffsetPoint.Y);
            Assert.AreEqual(15, _mgr.OffsetPoint.X);
        }

        [TestMethod]
        public void FindOffset_90Degrees()
        {
            _pcbManager.Setup(fid1 => fid1.FirstFiducial).Returns(new Core.Models.Drawing.Point2D<double>(0, 0));
            _pcbManager.Setup(fid2 => fid2.SecondFiducial).Returns(new Core.Models.Drawing.Point2D<double>(10, 10));


            _mgr.FirstLocated = new Core.Models.Drawing.Point2D<double>(0, 00);
            _mgr.SecondLocated = new Core.Models.Drawing.Point2D<double>(-10,10);

            Assert.AreEqual(-90, _mgr.RotationOffset.ToDegrees());
        }


    }
}
