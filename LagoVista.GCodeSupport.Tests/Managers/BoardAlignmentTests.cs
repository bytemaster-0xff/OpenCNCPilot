using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.GCode.Sender.Managers;
using LagoVista.GCodeSupport.Tests.Mocks;

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



        [TestMethod]
        public void TestMethod1()
        {
            _pcbManager.Setup(pcb => pcb.FirstFiducial).Returns(new EaglePCB.Models.Drill() { X = 7.5, Y = 7.5 });
            _pcbManager.Setup(pcb => pcb.SecondFiducial).Returns(new EaglePCB.Models.Drill() { X = 37.5, Y = 37.5 }); /* Difference is 30 */

            _boardAlignmentManager.AlignBoard();

            _machine.Setup(mach => mach.MachinePosition).Returns(new Core.Models.Drawing.Vector3(15.5, 15.5, 0));
            /* Assume Move/Jog has been complete and we are directly on the first point */
            _boardAlignmentManager.CircleLocated(new Core.Models.Drawing.Point2D<double>(0, 0));

            _machine.Setup(mach => mach.MachinePosition).Returns(new Core.Models.Drawing.Vector3(45.5, 45.5, 0));
            /* Assume Move/Jog has been complete and we are directly on the second point */
            _boardAlignmentManager.CircleLocated(new Core.Models.Drawing.Point2D<double>(0, 0));

        }
    }
}
