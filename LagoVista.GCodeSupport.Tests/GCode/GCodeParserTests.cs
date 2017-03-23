using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LagoVista.Core.GCode;
using System.Collections.Generic;
using System.Diagnostics;
using LagoVista.Core.GCode.Commands;
using LagoVista.Core.GCode.Parser;
using LagoVista.GCodeSupport.Tests.Mocks;

namespace LagoVista.GCodeSupport.Tests
{
    [TestClass]
    public class GCodeParserTests
    {

        [TestMethod]
        public void Parse_File()
        {
            var file = GCodeFile.FromString(GCodeSampleFile.GCODE_FILE, new FakeLogger());

            Console.WriteLine("Total Run Time MS: " + file.EstimatedRunTime);

            foreach(var cmd in file.Commands)
            {
                Console.WriteLine(cmd.ToString());
            }
        }

        [TestMethod]
        public void ShouldFindG0CommandAsG0()
        {
            var parser = new GCodeParser(new FakeLogger());
            var cmd = parser.ParseLine("G0 X0", 0) as GCodeMotion;
            Assert.AreEqual("G0", cmd.Command);
        }

        [TestMethod]
        public void ShouldFindG00CommandAsG0()
        {
            var parser = new GCodeParser(new FakeLogger());
            var cmd = parser.ParseLine("G00 X0", 0) as GCodeMotion;
            Assert.AreEqual("G0", cmd.Command);
        }

        [TestMethod]
        public void ShouldFindG1CommandAsG1()
        {
            var parser = new GCodeParser(new FakeLogger());
            var cmd = parser.ParseLine("G1 X0", 0) as GCodeMotion;
            Assert.AreEqual("G1", cmd.Command);
        }


        [TestMethod]
        public void ShouldFindG01CommandAsG1()
        {
            var parser = new GCodeParser(new FakeLogger());
            var cmd = parser.ParseLine("G01 X0", 0) as GCodeMotion;
            Assert.AreEqual("G1", cmd.Command);
        }

        [TestMethod]
        public void ShouldFindG0X0CommandAsG0()
        {
            var parser = new GCodeParser(new FakeLogger());
            var cmd = parser.ParseLine("G0X0", 0) as GCodeMotion;
            Assert.AreEqual("G0", cmd.Command);
        }

        [TestMethod]
        public void ShouldFindG0_X10CommandAsG0WithX10()
        {
            var parser = new GCodeParser(new FakeLogger());
            var cmd = parser.ParseLine("G0 X10", 0) as GCodeMotion;
            Assert.AreEqual(10, cmd.End.X);
        }

        [TestMethod]
        public void ShouldFindG0_Retain4Decimals()
        {
            var parser = new GCodeParser(new FakeLogger());
            var cmd = parser.ParseLine("G0 X10.4253", 0) as GCodeMotion;
            Assert.AreEqual(10.4253, cmd.End.X);
            Assert.AreEqual("G0 X10.4253", cmd.Line);
        }

        [TestMethod]
        public void ShouldFindG0_Retain1Decimal()
        {
            var parser = new GCodeParser(new FakeLogger());
            var cmd = parser.ParseLine("G0 X10.4", 0) as GCodeMotion;
            Assert.AreEqual(10.4, cmd.End.X);
            Assert.AreEqual("G0 X10.4", cmd.Line);
        }

        [TestMethod]
        public void ShouldFindG0_ReturnWHoleNumber()
        {
            var parser = new GCodeParser(new FakeLogger());
            var cmd = parser.ParseLine("G0 X10.00", 0) as GCodeMotion;
            Assert.AreEqual(10, cmd.End.X);
            Assert.AreEqual("G0 X10", cmd.Line);
        }
   
        [TestMethod]
        public void ShouldFindG0_X10_Y10CommandAsG0WithX10AndY10()
        {
            var parser = new GCodeParser(new FakeLogger());
            var cmd = parser.ParseLine("G0 X10 Y10", 0) as GCodeMotion;
            Assert.AreEqual(10, cmd.End.X);
            Assert.AreEqual(10, cmd.End.Y);
            Assert.AreEqual("G0 X10 Y10", cmd.Line);
        }

        [TestMethod]
        public void ShouldGenerateG1LineWithOnlyXMove()
        {
            var parser = new GCodeParser(new FakeLogger());
            parser.State.Position = new Core.Models.Drawing.Vector3(5, 5, 0);
            var cmd = parser.ParseLine("G0X10", 0) as GCodeMotion;
            Assert.AreEqual("G0 X10", cmd.Line);
        }

        [TestMethod]
        public void ShouldAddFeedIfNotPresent()
        {
            var parser = new GCodeParser(new FakeLogger());
            var cmd = parser.ParseLine("G0 X10 F300", 0) as GCodeMotion;
            Assert.AreEqual(10, cmd.End.X);
            Assert.AreEqual(300, cmd.Feed.Value);
            Assert.AreEqual("G0 X10 F300", cmd.Line);
        }

        [TestMethod]
        public void ShouldNotAddFeedIfNotChanged()
        {
            var parser = new GCodeParser(new FakeLogger());
            parser.State.Feed = 300;
            var cmd = parser.ParseLine("G0 X10 F300", 0) as GCodeMotion;
            Assert.AreEqual(10, cmd.End.X);
            Assert.AreEqual(300, cmd.Feed.Value);
            Assert.AreEqual("G0 X10", cmd.Line);
        }

        [TestMethod]
        public void ShouldAddFeedIfChanged()
        {
            var parser = new GCodeParser(new FakeLogger());
            parser.State.Feed = 300;
            var cmd = parser.ParseLine("G0 X10 F250", 0) as GCodeMotion;
            Assert.AreEqual(10, cmd.End.X);
            Assert.AreEqual(250, cmd.Feed.Value);
            Assert.AreEqual("G0 X10 F250", cmd.Line);
        }

        [TestMethod]
        public void ShouldAddSpindleIfNotPresent()
        {
            var parser = new GCodeParser(new FakeLogger());
            var cmd = parser.ParseLine("G0 X10 S30", 0) as GCodeMotion;
            Assert.AreEqual(10, cmd.End.X);
            Assert.AreEqual(30, cmd.SpindleRPM.Value);
            Assert.AreEqual("G0 X10 S30", cmd.Line);
        }

        [TestMethod]
        public void ShouldNotAddSpindleIfNotChanged()
        {
            var parser = new GCodeParser(new FakeLogger());
            parser.State.SpindleRPM = 30;
            var cmd = parser.ParseLine("G0 X10 S30", 0) as GCodeMotion;
            Assert.AreEqual(10, cmd.End.X);
            Assert.AreEqual(30, cmd.SpindleRPM.Value);
            Assert.AreEqual("G0 X10", cmd.Line);
        }

        [TestMethod]
        public void ShouldAddSpindleIfChanged()
        {
            var parser = new GCodeParser(new FakeLogger());
            parser.State.SpindleRPM = 30;
            var cmd = parser.ParseLine("G0 X10 S25", 0) as GCodeMotion;
            Assert.AreEqual(10, cmd.End.X);
            Assert.AreEqual(25, cmd.SpindleRPM.Value);
            Assert.AreEqual("G0 X10 S25", cmd.Line);
        }
    }
}
