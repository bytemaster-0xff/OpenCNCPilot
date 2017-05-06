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
    public class ApplyLineRotation
    {
        [TestMethod]
        public void ApplyZeroRotation()
        {
            var parser = new GCodeParser(new FakeLogger());
            var cmd = parser.ParseLine("G1 X30 Y0", 0) as GCodeLine;
            cmd.Rotate(0);
            Assert.AreEqual(30, cmd.End.X);
            Assert.AreEqual("G1 X30", cmd.Line);
        }

        [TestMethod]
        public void Apply90CC2Rotation()
        {
            var parser = new GCodeParser(new FakeLogger());
            var cmd = parser.ParseLine("G1 X30 Y0", 0) as GCodeLine;
            cmd.Rotate(90);
            Console.WriteLine(cmd.Line);
            Assert.AreEqual(30, cmd.End.Y);
            Assert.AreEqual("G1 Y30", cmd.Line);
        }

    }
}