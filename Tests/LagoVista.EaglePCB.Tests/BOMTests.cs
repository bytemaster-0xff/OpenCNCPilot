using LagoVista.EaglePCB.Managers;
using LagoVista.EaglePCB.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.EaglePCB.Tests
{
    [TestClass]
    public class BOMTests
    {
        [TestMethod]
        public void BuildBOMTest()
        {
            var doc = XDocument.Load("./KegeratorController.brd");

            var pcb = EagleParser.ReadPCB(doc);

            var bom = new BOM(pcb);
            foreach(var entry in bom.Entries.Where(entry=>entry.Package.IsSMD))
            {
                Console.WriteLine(entry);
            }

        }
    }
}
