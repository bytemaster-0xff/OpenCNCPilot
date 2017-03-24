using LagoVista.Core.GCode.Commands;
using LagoVista.Core.GCode.Parser;
using LagoVista.GCodeSupport.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCodeSupport.Tests.GCode
{
    [TestClass]
    public class SplitTests
    {
        [TestMethod]
        public void LineSplitTest()
        {
            var parser = new GCodeParser(new FakeLogger());
            var cmd = parser.ParseLine("G0 X25 Y25", 0) as GCodeMotion;
            Assert.AreEqual("G0", cmd.Command);

            var cmds = cmd.Split(5);

            var first = cmds.First();
            Assert.AreEqual(cmd.Start.X, first.Start.X);
            Assert.AreEqual(cmd.Start.Y, first.Start.Y);
            Assert.AreEqual(cmd.Start.Z, first.Start.Z);

            var last = cmds.Last();
            Assert.AreEqual(cmd.End.X, last.End.X);
            Assert.AreEqual(cmd.End.Y, last.End.Y);
            Assert.AreEqual(cmd.End.Z, last.End.Z);
        }
    }
}
