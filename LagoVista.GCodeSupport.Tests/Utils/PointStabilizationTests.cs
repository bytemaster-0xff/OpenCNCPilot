using LagoVista.GCode.Sender.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCodeSupport.Tests.Utils
{
    [TestClass]
    public class PointStabilizationTests
    {
        [TestInitialize]
        public void Init()
        {

        }

        [TestMethod]
        public void ShouldNotBeStabilizedAfterSinglePoint()
        {
            var filter = new PointStabilizationFilter(2, 5);
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            Assert.IsFalse(filter.HasStabilizedPoint);
        }

        [TestMethod]
        public void ShouldBeStabilizedAfterFiveOfSamePoint()
        {
            var filter = new PointStabilizationFilter(2, 5);
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            Assert.IsTrue(filter.HasStabilizedPoint);
        }


        [TestMethod]
        public void StabilizedPointShouldBeSameAsInputValue()
        {
            var filter = new PointStabilizationFilter(2, 5);
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            Assert.AreEqual(5, filter.StabilizedPoint.X);
            Assert.AreEqual(5, filter.StabilizedPoint.Y);
        }

        [TestMethod]
        public void ShouldNotBeStabilizedAfter4StablePointsAndOneOutOfTolerance()
        {
            var filter = new PointStabilizationFilter(2, 5);
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(8, 8));
            Assert.IsFalse(filter.HasStabilizedPoint);
        }

        [TestMethod]
        public void ShouldBeStabilizedIf6thPointInToleranceButDifferent()
        {
            var filter = new PointStabilizationFilter(2, 5);
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(6.1, 4.75));

            Assert.IsTrue(filter.HasStabilizedPoint);
        }


        [TestMethod]
        public void ShouldNotBeStabilizedIfStabilizedThen6thPointIsOutOfToleranceButDifferent()
        {
            var filter = new PointStabilizationFilter(2, 5);
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            filter.Add(new Core.Models.Drawing.Point2D<double>(5, 5));
            Assert.IsTrue(filter.HasStabilizedPoint);
            filter.Add(new Core.Models.Drawing.Point2D<double>(8, 4.75));

            Assert.IsFalse(filter.HasStabilizedPoint);
        }

    }
}
