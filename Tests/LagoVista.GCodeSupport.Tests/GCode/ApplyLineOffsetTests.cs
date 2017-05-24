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

        [TestMethod]
        public void ApplyXOffsetTest()
        {
            var parser = new GCodeParser(new FakeLogger());
            var cmd = parser.ParseLine("G1 X30", 10) as GCodeMotion;
            cmd.ApplyOffset(10, 0);
            Assert.AreEqual(40, cmd.End.X);
            Assert.AreEqual("G1 X40 Z0", cmd.Line);
        }


        [TestMethod]
        public void ApplyYOffsetTest()
        {
            var parser = new GCodeParser(new FakeLogger());
            var cmd = parser.ParseLine("G1 X0 Y30 Z0", 10) as GCodeMotion;
            cmd.ApplyOffset(0, 10);
            Assert.AreEqual(40, cmd.End.Y);
            Assert.AreEqual("G1 X0 Y40 Z0", cmd.Line);
        }
    }
}
