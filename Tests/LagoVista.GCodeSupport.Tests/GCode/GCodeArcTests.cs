using LagoVista.Core.GCode.Commands;
using LagoVista.Core.GCode.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LagoVista.Core;
using LagoVista.Core.GCode;
using LagoVista.GCodeSupport.Tests.Mocks;

namespace LagoVista.GCodeSupport.Tests
{
    [TestClass]
    public class GCodeArcTests
    {
        //TODO: Really need to beef up the tests for splitting arc and rendering line


        [TestMethod]
        public void SplitArc()
        {
            var gcode = new List<string>();
            gcode.Add("G00 X9.0 Y1.4000 F200");
            gcode.Add("G00 X97.0000 Y1.4000 F200");
            gcode.Add("G03 X104.6000 Y9.0000 R7.6");
            /*gcode.Add("G00 X104.6000 Y77.0000");
            gcode.Add("G03 X97.0000 Y84.6000 R7.6");
            gcode.Add("G0 X9.0000 Y84.6000");
            gcode.Add("G03 X1.4000 Y77.0000 R7.6");
            gcode.Add("G03 X9.0000 Y1.4000 R7.6");*/

            //gcode.Add("G00 X45 Y0");
            //gcode.Add("G03 X50 Y5 R5");

            var parser = new GCodeParser(new FakeLogger());
            parser.Parse(gcode);

            foreach (var cmd in parser.Commands)
            {
                if(cmd is GCodeArc)
                {
                    var arc = cmd as GCodeArc;
                    arc.U = 97;
                    arc.V = 9;
                }
            }
        }

        [TestMethod]
        public void Parse_CCW_ArcTest()
        {
            var parser = new GCodeParser(new FakeLogger());
            parser.State.Position.X = 45;
            parser.State.Position.Y = 3;

            var words = new List<Word>();

            words.Add(new Word() { Command = 'R', Parameter = 5 });
            Debug.WriteLine("Bottom Right Corner");
            Debug.WriteLine("================");

            var arc = parser.ParseArc(words, 3, new Core.Models.Drawing.Vector3(50, 8, 0), 1);
            Debug.WriteLine(arc.U);
            Debug.WriteLine(arc.V);

            Debug.WriteLine(arc.Start);
            Debug.WriteLine(arc.End);

            Assert.AreEqual(45, arc.U);
            Assert.AreEqual(8, arc.V);
            Debug.WriteLine("  ");

            /*-----*/
            Debug.WriteLine("Top Right Corner");
            Debug.WriteLine("================");

            parser.State.Position.X = 50;
            parser.State.Position.Y = 45;
            words.Add(new Word() { Command = 'R', Parameter = 5 });

            var arc2 = parser.ParseArc(words, 3, new Core.Models.Drawing.Vector3(45, 50, 0), 1);
            Debug.WriteLine(arc2.U);
            Debug.WriteLine(arc2.V);

            Debug.WriteLine(arc2.Start);
            Debug.WriteLine(arc2.End);

            Assert.AreEqual(45, arc2.U);
            Assert.AreEqual(45, arc2.V);
            Debug.WriteLine("  ");

            /*-----*/
            Debug.WriteLine("Top Left Corner");
            Debug.WriteLine("================");

            parser.State.Position.X = 5;
            parser.State.Position.Y = 50;
            words.Add(new Word() { Command = 'R', Parameter = 5 });

            var arc3 = parser.ParseArc(words, 3, new Core.Models.Drawing.Vector3(0, 45, 0), 1);

            Debug.WriteLine(arc3.U);
            Debug.WriteLine(arc3.V);

            Debug.WriteLine(arc3.Start);
            Debug.WriteLine(arc3.End);

            Assert.AreEqual(5, arc3.U);
            Assert.AreEqual(45, arc3.V);
            Debug.WriteLine("  ");

            /*-----*/
            Debug.WriteLine("Bottom Left Corner");
            Debug.WriteLine("================");

            parser.State.Position.X = 0;
            parser.State.Position.Y = 5;
            words.Add(new Word() { Command = 'R', Parameter = 5 });

            var arc4 = parser.ParseArc(words, 3, new Core.Models.Drawing.Vector3(5, 0, 0), 1);
            Debug.WriteLine(arc4.U);
            Debug.WriteLine(arc4.V);

            Debug.WriteLine(arc4.Start);
            Debug.WriteLine(arc4.End);

            Assert.AreEqual(5, arc4.U);
            Assert.AreEqual(5, arc4.V);
            Debug.WriteLine("  ");
        }
    }
}
