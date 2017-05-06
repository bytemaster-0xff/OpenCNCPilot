using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using LagoVista.PickAndPlace.Models;

namespace LagoVista.PickAndPlace.Tests
{
    [TestClass]
    public class JOBTests
    {
        [TestMethod]
        public async void CreateBOMTests()
        {
            var job = new PnPJob();
            job.EagleBRDFilePath = "./KegeratorController.brd";

            await job.OpenAsync();


        }
    }
}
