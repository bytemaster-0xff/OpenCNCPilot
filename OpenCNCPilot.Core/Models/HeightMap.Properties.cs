using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;

namespace LagoVista.GCode.Sender.Models
{
    public partial class HeightMap
    {
        public double?[,] Points { get; private set; }
        public int SizeX { get; private set; }
        public int SizeY { get; private set; }

        public int Progress { get { return TotalPoints - NotProbed.Count; } }
        public int TotalPoints { get { return SizeX * SizeY; } }

        public Queue<Tuple<int, int>> NotProbed { get; private set; } = new Queue<Tuple<int, int>>();
        public Vector2 Min { get; private set; }
        public Vector2 Max { get; private set; }

        public Vector2 Delta { get { return Max - Min; } }

        public double MinHeight { get; private set; } = 0;
        public double MaxHeight { get; private set; } = 0;

        public double GridX { get { return (Max.X - Min.X) / (SizeX - 1); } }
        public double GridY { get { return (Max.Y - Min.Y) / (SizeY - 1); } }
    }
}
