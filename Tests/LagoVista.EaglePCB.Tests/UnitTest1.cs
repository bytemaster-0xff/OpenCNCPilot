using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using System.Linq;
using LagoVista.EaglePCB.Managers;
using System.Diagnostics;

namespace LagoVista.EaglePCB.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

            var doc = XDocument.Load("./KegeratorController.brd");

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
            foreach(var drill in drills)
            {
                Debug.WriteLine($"\t{drill}");
            }
        }
    }
}
