using LagoVista.Core.GCode.Commands;
using LagoVista.Core.GCode.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCodeSupport.Tests.GCode
{
    [TestClass]
    public class LineParser
    {
        [TestMethod]
        public void ParseSetSpindle()
        {
            var parser = new GCodeParser();
            var line = "S20000";
            var cmd = parser.ParseLine(line, 0);
        }

        [TestMethod]
        public void ParseChangeTool()
        {
            var parser = new GCodeParser();
            var line = "M06 T01; 0.8000";
            var cmd = parser.ParseLine(line, 0);
            Console.WriteLine((cmd as MCode).DrillSize);
        }
    }
}
