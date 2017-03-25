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

        IBoardAlignmentManager _boardAlignmentManager;

        [TestInitialize]
        public void Init()

        {
            _machine = new Moq.Mock<IMachine>();
            _pcbManager = new Moq.Mock<IPCBManager>();
            _boardAlignmentManager = new BoardAlignmentManager(_machine.Object, new FakeLogger(),  _pcbManager.Object);
        }




        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
