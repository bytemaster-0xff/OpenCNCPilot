using LagoVista.GCode.Sender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class NewHeightMapViewModel
    {
        private HeightMap _heightMap;
        public HeightMap HeightMap
        {
            get { return _heightMap; }
            set { Set(ref _heightMap, value); }
        }

        public bool Validate()
        {
            if (HeightMap.Min.X > HeightMap.Max.X)
            {
                var originalMinX = HeightMap.Min.X;
                HeightMap.Min = new Core.Models.Drawing.Vector2()
                {
                    X = HeightMap.Max.X,
                    Y = HeightMap.Min.Y
                };

                HeightMap.Max = new Core.Models.Drawing.Vector2()
                {
                    X = originalMinX,
                    Y = HeightMap.Max.Y
                };
            }

            if (HeightMap.Min.Y > HeightMap.Max.Y)
            {
                var originalMinY = HeightMap.Min.Y;
                HeightMap.Min = new Core.Models.Drawing.Vector2()
                {
                    X = HeightMap.Min.X,
                    Y = HeightMap.Max.Y
                };

                HeightMap.Max = new Core.Models.Drawing.Vector2()
                {
                    X = HeightMap.Max.Y,
                    Y = originalMinY
                };
            }
            return true;
        }


        public void GenerateTestPattern()
        {
             
        }
    }
}
