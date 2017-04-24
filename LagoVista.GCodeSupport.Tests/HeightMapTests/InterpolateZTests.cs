using LagoVista.GCode.Sender.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCodeSupport.Tests.HeightMapTests
{
    [TestClass]
    public class InterpolateZTests
    {
        HeightMap _heightMap;

        public void Init()
        {
            _heightMap = new HeightMap(null, null);

            _heightMap.GridSize = 10;
            _heightMap.SizeX = 2;
            _heightMap.SizeY = 2;
            _heightMap.Min = new Core.Models.Drawing.Vector2(0, 0);
            _heightMap.Max = new Core.Models.Drawing.Vector2(10, 10);
        }

        private void AddPoint(int xIndex, int yIndex, double x, double y, double z)
        {
            _heightMap.Points.Add(new HeightMapProbePoint()
            {
                Point = new Core.Models.Drawing.Vector3(x, y, z),
                XIndex = xIndex,
                YIndex = yIndex,
                Status = HeightMapProbePointStatus.Probed

            });
        }

        [TestMethod]
        public void Quardrant1Test()
        {
            /* Low point - bottom left, hight point - top right */
            Init();
            AddPoint(0, 0, 0, 0, 0);
            AddPoint(0, 1, 0, 10, 0.5);
            AddPoint(1, 0, 10, 0, 0.5);
            AddPoint(1, 1, 10, 10, 1);

            Assert.AreEqual(0.5, _heightMap.InterpolateZ(5, 5));
            Assert.AreEqual(0, _heightMap.InterpolateZ(0, 0));
            Assert.AreEqual(1, _heightMap.InterpolateZ(10, 10));
            Assert.AreEqual(0.25, _heightMap.InterpolateZ(0, 5));
        }


        [TestMethod]
        public void Quardrant2Test()
        {
            /* Low point - Left Middle, hight point right middle*/
            Init();
            AddPoint(0, 0, 0, 0, 0); /* BL */
            AddPoint(0, 1, 0, 10, 0); /* TL */
            AddPoint(1, 0, 10, 0, 1); /* BR */
            AddPoint(1, 1, 10, 10, 1); /* TR */

            Assert.AreEqual(0.5, _heightMap.InterpolateZ(5, 5));
            Assert.AreEqual(0, _heightMap.InterpolateZ(0, 0));
            Assert.AreEqual(1, _heightMap.InterpolateZ(10, 10));
            Assert.AreEqual(0, _heightMap.InterpolateZ(0, 5));
            Assert.AreEqual(1, _heightMap.InterpolateZ(10, 5));
            Assert.AreEqual(0.5,  _heightMap.InterpolateZ(5, 0));
            Assert.AreEqual(0.5, _heightMap.InterpolateZ(5, 10));
        }
    }
}
