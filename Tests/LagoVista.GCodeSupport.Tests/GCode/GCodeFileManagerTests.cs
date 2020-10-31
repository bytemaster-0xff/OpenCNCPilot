using LagoVista.Core.PlatformSupport;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.GCode.Sender.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using LagoVista.Core.GCode.Commands;

namespace LagoVista.GCodeSupport.Tests.GCode
{
    [TestClass]
    public class GCodeFileManagerTests
    {
        Moq.Mock<IMachine> _machine;
        Moq.Mock<ILogger> _logger;
        Moq.Mock<IToolChangeManager> _toolChangeManager;

        [TestInitialize]
        public void Setup()
        {
            _machine = new Moq.Mock<IMachine>();
            _logger = new Mock<ILogger>();
            _toolChangeManager = new Mock<IToolChangeManager>();
        }

        [TestMethod]
        public void ShouldSendNextLineIfLBufferSpacevailable()
        {
            var mgr = new GCodeFileManager(_machine.Object, _logger.Object, _toolChangeManager.Object);
            mgr.SetGCode(GCodeSampleFile.GCODE_FILE);

            _machine.Setup(mac => mac.HasBufferSpaceAvailableForByteCount(It.IsAny<int>())).Returns(true);

            mgr.GetNextJobItem();

            _machine.Verify(mac => mac.SendJobCommand(It.IsAny<GCodeCommand>()));
        }

        [TestMethod]
        public void ShouldNotSendNextLineIfBufferSpaceNotAvailable()
        {
            var mgr = new GCodeFileManager(_machine.Object, _logger.Object, _toolChangeManager.Object);
            mgr.SetGCode(GCodeSampleFile.GCODE_FILE);

            _machine.Setup(mac => mac.HasBufferSpaceAvailableForByteCount(It.IsAny<int>())).Returns(false);

            mgr.GetNextJobItem();

            _machine.Verify(mac => mac.SendJobCommand(It.IsAny<GCodeCommand>()), Times.Never);
        }
    }
}
