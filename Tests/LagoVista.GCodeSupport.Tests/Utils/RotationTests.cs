using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LagoVista.Core.Models.Drawing;

namespace LagoVista.GCodeSupport.Tests.Utils
{

    [TestClass]
    public class RotationTests
    {
        [TestMethod]
        public void ShouldRotateCCW()
        {
            var point = new Point2D<double>(0, -100);

            var result = point.Rotate(90);

            Assert.AreEqual(100,Math.Round( result.X));
            Assert.AreEqual(00, Math.Round( result.Y, 4));

            point = new Point2D<double>(100, 00);

            result = point.Rotate(90);

            Assert.AreEqual(0, Math.Round(result.X), 4);
            Assert.AreEqual(100, Math.Round(result.Y, 4));

        }
    }
}
