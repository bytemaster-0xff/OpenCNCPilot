using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using System.Linq;
using LagoVista.EaglePCB.Managers;

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

            
        }
    }
}
