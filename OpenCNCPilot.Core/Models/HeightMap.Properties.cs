using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;

namespace LagoVista.GCode.Sender.Models
{
    public partial class HeightMap
    {
        public double?[,] Points { get;  set; }
        public int SizeX { get;  set; }
        public int SizeY { get;  set; }

        public int Progress { get { return TotalPoints - NotProbed.Count; } }
        public int TotalPoints { get { return SizeX * SizeY; } }

        public Queue<Tuple<int, int>> NotProbed { get; private set; } = new Queue<Tuple<int, int>>();

        private Vector2 _min;
        public Vector2 Min
        {
            get { return _min; }
            set { Set(ref _min, value); }
        }

        private Vector2 _max;
        public Vector2 Max
        {
            get { return _max; }
            set { Set(ref _max, value); }
        }

        private double _gridSize;
        public double GridSize
        {
            get { return _gridSize; }
            set { Set(ref _gridSize, value); }
        }

        public Vector2 Delta { get { return Max - Min; } }

        public double MinHeight { get;  set; } = 0;
        public double MaxHeight { get; set; } = 0;

        public double GridX { get { return (Max.X - Min.X) / (SizeX - 1); } }
        public double GridY { get { return (Max.Y - Min.Y) / (SizeY - 1); } }
    }
}
