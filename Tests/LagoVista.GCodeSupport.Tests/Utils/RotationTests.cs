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

        [TestMethod]
        public void ShouldRotate0_At_Origin()
        {
            var point = new Point2D<double>(100, 0);

            var result = point.Rotate(new Point2D<double>(0,0), 0);

            Assert.AreEqual(100, Math.Round(result.X));
            Assert.AreEqual(0, Math.Round(result.Y, 4));
        }


        [TestMethod]
        public void ShouldRotate_90_At_Origin()
        {
            var point = new Point2D<double>(100, 0);

            var result = point.Rotate(new Point2D<double>(0, 0), 90);

            Assert.AreEqual(100, Math.Round(result.Y));
            Assert.AreEqual(0, Math.Round(result.X, 4));
        }

        [TestMethod]
        public void ShouldRotate_180_At_Origin()
        {
            var point = new Point2D<double>(100, 0);

            var result = point.Rotate(new Point2D<double>(0, 0), 180);

            Assert.AreEqual(00, Math.Round(result.Y, 4));
            Assert.AreEqual(-100, Math.Round(result.X, 4));
        }


        [TestMethod]
        public void ShouldRotate_270_At_Origin()
        {
            var point = new Point2D<double>(100, 0);

            var result = point.Rotate(new Point2D<double>(0, 0), 270);

            Assert.AreEqual(00, Math.Round(result.X, 4));
            Assert.AreEqual(-100, Math.Round(result.Y, 4));
        }


        [TestMethod]
        public void ShouldRotate0_At_5x5()
        {
            var point = new Point2D<double>(00, 00);

            var result = point.Rotate(new Point2D<double>(5, 5), 0);

            Assert.AreEqual(00, Math.Round(result.X));
            Assert.AreEqual(00, Math.Round(result.Y, 4));
        }

        [TestMethod]
        public void ShouldRotate0_5x5_around_2x2()
        {
            var point = new Point2D<double>(5, 5);

            var result = point.Rotate(new Point2D<double>(2, 2), 0);

            Assert.AreEqual(5, Math.Round(result.X));
            Assert.AreEqual(5, Math.Round(result.Y, 4));
        }


        [TestMethod]
        public void ShouldRotate90_5x5_Around_2x2()
        {
            var point = new Point2D<double>(5, 5);

            var result = point.Rotate(new Point2D<double>(2, 2), 90);

            Assert.AreEqual(5, Math.Round(result.Y, 2));
            Assert.AreEqual(-1, Math.Round(result.X, 2));
        }

        [TestMethod]
        public void ShouldRotate90_1x1_Around_2x2()
        {
            var point = new Point2D<double>(4, 4);

            var result = point.Rotate(new Point2D<double>(2, 2), 90);

            Assert.AreEqual(4, Math.Round(result.Y, 4));
            Assert.AreEqual(0, Math.Round(result.X, 0));
        }
    }
}
