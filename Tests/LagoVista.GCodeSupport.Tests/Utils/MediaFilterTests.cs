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
    public class MediaFilterTests
    {
        [TestMethod]
        public void NullValueTest()
        {
            var imf = new IntMedianFilter();

            Assert.IsNull(imf.Filtered);
        }


        [TestMethod]
        public void OneValueShouldReturnThatAsAverage()
        {
            var imf = new IntMedianFilter();
            imf.Add(new Core.Models.Drawing.Point2D<int>(5, 8));
            Assert.AreEqual(5,imf.Filtered.X);
            Assert.AreEqual(8, imf.Filtered.Y);
        }

        [TestMethod]
        public void TwoValueShouldReturnAverageOfTwoValues()
        {
            var imf = new IntMedianFilter();
            imf.Add(new Core.Models.Drawing.Point2D<int>(4, 8));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            Assert.AreEqual(6, imf.Filtered.X);
            Assert.AreEqual(10, imf.Filtered.Y);
        }

        [TestMethod]
        public void FitlerSizeValueShouldReturnAverageOfNMedianValues()
        {
            var imf = new IntMedianFilter(12);

            /* Add twelve of same items, should return same value back as average */
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));

            Assert.AreEqual(8, imf.Filtered.X);
            Assert.AreEqual(12, imf.Filtered.Y);
        }

        [TestMethod]
        public void ShouldAdd14ValuesAndNotBlowUpInt()
        {
            var imf = new IntMedianFilter(12);

            /* Add twelve of same items, should return same value back as average */
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));


            Assert.AreEqual(8, imf.Filtered.X);
            Assert.AreEqual(12, imf.Filtered.Y);
        }


        [TestMethod]
        public void ShouldThrowOutThreeHightestValues()
        {
            var imf = new IntMedianFilter(12,3);

            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 99));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(32, 12));

            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 16));
            imf.Add(new Core.Models.Drawing.Point2D<int>(34, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(89, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 42));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));

            Assert.AreEqual(8, imf.Filtered.X);
            Assert.AreEqual(12, imf.Filtered.Y);
        }

        [TestMethod]
        public void ShouldThrowOutThreeLowstValues()
        {
            var imf = new IntMedianFilter(12, 3);

            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(3, 7));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(2, 9));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 6));
            imf.Add(new Core.Models.Drawing.Point2D<int>(5, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));

            Assert.AreEqual(8, imf.Filtered.X);
            Assert.AreEqual(12, imf.Filtered.Y);
        }

        [TestMethod]
        public void ShouldThrowOutLowestAndHighestValues()
        {
            var imf = new IntMedianFilter(12);

            /* Add twelve of same items, should return same value back as average */
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(3, 8));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(14, 21));

            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(6, 44));
            imf.Add(new Core.Models.Drawing.Point2D<int>(44, 9));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 32));
            imf.Add(new Core.Models.Drawing.Point2D<int>(32, 12));
            imf.Add(new Core.Models.Drawing.Point2D<int>(2, 10));
            imf.Add(new Core.Models.Drawing.Point2D<int>(8, 12));

            Assert.AreEqual(8, imf.Filtered.X);
            Assert.AreEqual(12, imf.Filtered.Y);
        }

        [TestMethod]
        public void ShouldAverageMiddle6Values()
        {
            var imf = new IntMedianFilter(12);

            /* Add twelve of same items, should return same value back as average */
            imf.Add(new Core.Models.Drawing.Point2D<int>(6, 10));
            imf.Add(new Core.Models.Drawing.Point2D<int>(3, 8));
            imf.Add(new Core.Models.Drawing.Point2D<int>(10, 14));
            imf.Add(new Core.Models.Drawing.Point2D<int>(14, 21));

            imf.Add(new Core.Models.Drawing.Point2D<int>(10, 14));
            imf.Add(new Core.Models.Drawing.Point2D<int>(5, 44));
            imf.Add(new Core.Models.Drawing.Point2D<int>(44, 9));
            imf.Add(new Core.Models.Drawing.Point2D<int>(10, 10));

            imf.Add(new Core.Models.Drawing.Point2D<int>(6, 32));
            imf.Add(new Core.Models.Drawing.Point2D<int>(32, 14));
            imf.Add(new Core.Models.Drawing.Point2D<int>(2, 10));
            imf.Add(new Core.Models.Drawing.Point2D<int>(6, 10));

            Assert.AreEqual(8, imf.Filtered.X);
            Assert.AreEqual(12, imf.Filtered.Y);
        }

        [TestMethod]
        public void NullValueTestFloat()
        {
            var imf = new FloatMedianFilter();

            Assert.IsNull(imf.Filtered);
        }


        [TestMethod]
        public void OneValueShouldReturnThatAsAverageFloat()
        {
            var imf = new FloatMedianFilter();
            imf.Add(new Core.Models.Drawing.Point2D<float>(5, 8));
            Assert.AreEqual(5, imf.Filtered.X);
            Assert.AreEqual(8, imf.Filtered.Y);
        }

        [TestMethod]
        public void TwoValueShouldReturnAverageOfTwoValuesFloat()
        {
            var imf = new FloatMedianFilter();
            imf.Add(new Core.Models.Drawing.Point2D<float>(4, 8));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            Assert.AreEqual(6, imf.Filtered.X);
            Assert.AreEqual(10, imf.Filtered.Y);
        }

        [TestMethod]
        public void FitlerSizeValueShouldReturnAverageOfNMedianValuesFloat()
        {
            var imf = new FloatMedianFilter(12);

            /* Add twelve of same items, should return same value back as average */
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));

            Assert.AreEqual(8, imf.Filtered.X);
            Assert.AreEqual(12, imf.Filtered.Y);
        }

        [TestMethod]
        public void Add14ValueAndDontOvershootArrayFloat()
        {
            var imf = new FloatMedianFilter(12);

            /* Add twelve of same items, should return same value back as average */
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));

            Assert.AreEqual(8, imf.Filtered.X);
            Assert.AreEqual(12, imf.Filtered.Y);
        }


        [TestMethod]
        public void ShouldThrowOutThreeHightestValuesFloat()
        {
            var imf = new FloatMedianFilter(12, 3);

            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 99));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(32, 12));

            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 16));
            imf.Add(new Core.Models.Drawing.Point2D<float>(34, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(89, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 42));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));

            Assert.AreEqual(8, imf.Filtered.X);
            Assert.AreEqual(12, imf.Filtered.Y);
        }

        [TestMethod]
        public void ShouldThrowOutThreeLowstValuesFloat()
        {
            var imf = new FloatMedianFilter(12, 3);

            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(3, 7));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(2, 9));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 6));
            imf.Add(new Core.Models.Drawing.Point2D<float>(5, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));

            Assert.AreEqual(8, imf.Filtered.X);
            Assert.AreEqual(12, imf.Filtered.Y);
        }

        [TestMethod]
        public void ShouldThrowOutLowestAndHighestValuesFloat()
        {
            var imf = new FloatMedianFilter(12);

            /* Add twelve of same items, should return same value back as average */
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(3, 8));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(14, 21));

            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(6, 44));
            imf.Add(new Core.Models.Drawing.Point2D<float>(44, 9));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));

            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 32));
            imf.Add(new Core.Models.Drawing.Point2D<float>(32, 12));
            imf.Add(new Core.Models.Drawing.Point2D<float>(2, 10));
            imf.Add(new Core.Models.Drawing.Point2D<float>(8, 12));

            Assert.AreEqual(8, imf.Filtered.X);
            Assert.AreEqual(12, imf.Filtered.Y);
        }

        [TestMethod]
        public void ShouldAverageMiddle6ValuesFloat()
        {
            var imf = new FloatMedianFilter(12);

            /* Add twelve of same items, should return same value back as average */
            imf.Add(new Core.Models.Drawing.Point2D<float>(6, 10));
            imf.Add(new Core.Models.Drawing.Point2D<float>(3, 8));
            imf.Add(new Core.Models.Drawing.Point2D<float>(10, 14));
            imf.Add(new Core.Models.Drawing.Point2D<float>(14, 21));

            imf.Add(new Core.Models.Drawing.Point2D<float>(10, 14));
            imf.Add(new Core.Models.Drawing.Point2D<float>(5, 44));
            imf.Add(new Core.Models.Drawing.Point2D<float>(44, 9));
            imf.Add(new Core.Models.Drawing.Point2D<float>(10, 10));

            imf.Add(new Core.Models.Drawing.Point2D<float>(6, 32));
            imf.Add(new Core.Models.Drawing.Point2D<float>(32, 14));
            imf.Add(new Core.Models.Drawing.Point2D<float>(2, 10));
            imf.Add(new Core.Models.Drawing.Point2D<float>(6, 10));

            Assert.AreEqual(8, imf.Filtered.X);
            Assert.AreEqual(12, imf.Filtered.Y);
        }


    }
}
