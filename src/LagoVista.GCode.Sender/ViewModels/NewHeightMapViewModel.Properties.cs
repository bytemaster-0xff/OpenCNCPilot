using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class NewHeightMapViewModel
    {
        public double MinX
        {
            get { return HeightMap.Min.X; }
            set { HeightMap.Min = new Core.Models.Drawing.Vector2(value, HeightMap.Min.Y); }
        }

        public double MinY
        {
            get { return HeightMap.Min.Y; }
            set { HeightMap.Min = new Core.Models.Drawing.Vector2(HeightMap.Min.Y, value); }
        }

        public double MaxX
        {
            get { return HeightMap.Max.X; }
            set { HeightMap.Max = new Core.Models.Drawing.Vector2(value, HeightMap.Max.Y); }
        }

        public double MaxY
        {
            get { return HeightMap.Max.Y; }
            set { HeightMap.Max = new Core.Models.Drawing.Vector2( HeightMap.Max.X, value); }
        }

        public double GridSize
        {
            get { return HeightMap.GridSize; }
            set { HeightMap.GridSize = value; }
        }



    }
}
