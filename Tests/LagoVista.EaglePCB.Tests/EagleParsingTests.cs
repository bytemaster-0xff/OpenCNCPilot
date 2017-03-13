using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using System.Linq;
using LagoVista.EaglePCB.Managers;
using System.Diagnostics;
using LagoVista.EaglePCB.Models;

namespace LagoVista.EaglePCB.Tests
{
    [TestClass]
    public class EagleParsingTests
    {
        [TestMethod]
        public void ParsePCB()
        {
            var doc = XDocument.Load("./EagleSample.brd");

            var pcb = EagleParser.ReadPCB(doc);

            var holes = pcb.Layers.Where(layer => layer.Number == 45).FirstOrDefault().Holes;
            var drills = pcb.Layers.Where(layer => layer.Number == 44).FirstOrDefault().Drills;
            var outlineWires = pcb.Layers.Where(layer => layer.Number == 20).FirstOrDefault().Wires;

            Debug.WriteLine("Outline");
            foreach (var outline in outlineWires)
            {
                Debug.WriteLine($"\t{outline}");
            }

            Debug.WriteLine("Holes");
            foreach (var hole in holes)
            {
                Debug.WriteLine($"\t{hole}");
            }

            Debug.WriteLine("Drills");
            foreach (var drill in drills)
            {
                Debug.WriteLine($"\t{drill}");
            }

            Debug.Write($"Width={pcb.Width}  Height={pcb.Height}");
        }

        [TestMethod]
        public void GenerateDrills()
        {
            var doc = XDocument.Load("./EagleSample.brd");
            var pcb = EagleParser.ReadPCB(doc);

            var config = new PCBProject()
            {
                PauseForToolChange = true,
                DrillSpindleRPM = 25000,
                DrillSpindleDwell = 3,
                DrillSafeHeight = 3,
                StockThickness = 1.75,
                Scrap = 3,
                DrillPlungeRate = 200,
                SafePlungeRecoverRate = 1000,               
            };

            var gcode = GCodeEngine.CreateDrillGCode(pcb, config);

            Debug.WriteLine(gcode);
        }

        [TestMethod]
        public void GenerateCutout()
        {
            var doc = XDocument.Load("./EagleSample.brd");
            var pcb = EagleParser.ReadPCB(doc);

            var config = new PCBProject()
            {
                PauseForToolChange = true,
                MillFeedRate = 200,
                MillCutDepth = 0.5,
                MillPlungeRate = 200,
                MillSafeHeight = 10,
                MillSpindleDwell = 3,
                MillSpindleRPM = 15000,
                MillToolSize = 3.2,
                StockThickness = 1.75,
                Scrap = 3,
                DrillPlungeRate = 200,
                SafePlungeRecoverRate = 1000,
            };

            var gcode = GCodeEngine.CreateCutoutMill(pcb, config);

            Debug.WriteLine(gcode);

        }

        [TestMethod]
        public void GetTraceSegments()
        {
               var doc = XDocument.Load("./PNPHeadBoard.brd");
            //var doc = XDocument.Load("./EagleSample.brd");
            var pcb = EagleParser.ReadPCB(doc);

            foreach(var signal in pcb.Signals)
            {
                Debug.WriteLine(signal.Name);
                foreach(var trace in signal.TopTraces)
                {
                    Debug.WriteLine("TRACE");
                    foreach(var wire in trace.Wires)
                    {
                        Debug.WriteLine("    " + wire.ToString());
                    }
                }
            }

        }
    }
}
