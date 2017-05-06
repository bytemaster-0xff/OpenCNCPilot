using LagoVista.Core.GCode.Commands;
using LagoVista.Core.GCode.Parser;
using LagoVista.GCodeSupport.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LagoVista.GCodeSupport.Tests.GCode
{
    [TestClass]
    public class LineParser
    {
        [TestMethod]
        public void ParseSetSpindle()
        {
            var parser = new GCodeParser(new FakeLogger());
            var line = "S20000";
            var cmd = parser.ParseLine(line, 0) as OtherCode;
        }

        [TestMethod]
        public void ParseChangeTool()
        {
            var parser = new GCodeParser(new FakeLogger());
            var line = "M06 T01; 0.8000";
            var cmd = parser.ParseLine(line, 0) as ToolChangeCommand;
            Assert.AreEqual("0.8000", cmd.ToolSize);
        }
    }
}
