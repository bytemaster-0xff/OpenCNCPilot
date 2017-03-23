using LagoVista.Core.GCode.Commands;
using LagoVista.Core.GCode.Parser;
using LagoVista.GCodeSupport.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCodeSupport.Tests.GCode
{
    [TestClass]
    public class ApplyLineOffsetTests
    {

        private GCodeLine GetGCodeLineWithOffset(String line, double xOffset = 0, double yOffset = 0, double zOffset = 0, double angle = 0, double lastX = 50, double lastY = 50, double lastZ= 5, double lastFeed = 300)
        {
            var parser = new GCodeParser(new FakeLogger());
            parser.State.Position = new Core.Models.Drawing.Vector3(lastX, lastY, lastZ);
            parser.State.Feed = lastFeed;
            var cmd = parser.ParseLine(line, 0) as GCodeLine;
            cmd.ApplyOffset(xOffset, yOffset, zOffset, angle);
            return cmd;
        }

        [TestMethod]
        public void ApplyXOffsetTestG1()
        {
            var cmd = GetGCodeLineWithOffset("G1 X30", 10);
            Assert.AreEqual(40, cmd.End.X);
            Assert.AreEqual("G1 X40", cmd.Line);
        }

        [TestMethod]
        public void ApplyXOffsetTestG01()
        {
            var cmd = GetGCodeLineWithOffset("G01 X30", 10);
            Assert.AreEqual(40, cmd.End.X);
            Assert.AreEqual("G1 X40", cmd.Line);
        }

        [TestMethod]
        public void ApplyXOffsetTestG0()
        {
            var cmd = GetGCodeLineWithOffset("G0 X30", 10);
            Assert.AreEqual(40, cmd.End.X);
            Assert.AreEqual("G0 X40", cmd.Line);
        }

        [TestMethod]
        public void ApplyXOffsetTestG00()
        {
            var cmd = GetGCodeLineWithOffset("G00 X30", 10);
            Assert.AreEqual(40, cmd.End.X);
            Assert.AreEqual("G0 X40", cmd.Line);
        }
    }
}
