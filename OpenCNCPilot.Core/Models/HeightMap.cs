using System;
using System.Collections.Generic;
using LagoVista.Core.GCode;
using LagoVista.Core.GCode.Commands;
using LagoVista.Core.Models.Drawing;

namespace LagoVista.GCode.Sender.Models
{
    public partial class HeightMap
    {
        private String _fileName = null;

        private Settings _settings;

        public HeightMap(Settings settings, double gridSize, Vector2 min, Vector2 max)
        {
            _settings = settings;

            Init(gridSize, min, max);
        }

        public HeightMap(Settings settings)
        {
            _settings = settings;
        }

        public HeightMap()
        {

        }

        private void Init(double gridSize, Vector2 min, Vector2 max)
        {
            if (min.X == max.X || min.Y == max.Y)
                throw new Exception("Height map can't be infinitely narrow");

            int pointsX = (int)Math.Ceiling((max.X - min.X) / gridSize) + 1;
            int pointsY = (int)Math.Ceiling((max.Y - min.Y) / gridSize) + 1;

            if (pointsX == 0 || pointsY == 0)
                throw new Exception("Height map must have at least 4 points");

            Points = new double?[pointsX, pointsY];

            if (max.X < min.X)
            {
                double a = min.X;
                min.X = max.X;
                max.X = a;
            }

            if (max.Y < min.Y)
            {
                double a = min.Y;
                min.Y = max.Y;
                max.Y = a;
            }

            Min = min;
            Max = max;

            SizeX = pointsX;
            SizeY = pointsY;

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                    NotProbed.Enqueue(new Tuple<int, int>(x, y));

                if (++x >= SizeX)
                    break;

                for (int y = SizeY - 1; y >= 0; y--)
                    NotProbed.Enqueue(new Tuple<int, int>(x, y));
            }
        }
  
        public Vector2 GetCoordinates(int x, int y)
        {
            return new Vector2(x * (Delta.X / (SizeX - 1)) + Min.X, y * (Delta.Y / (SizeY - 1)) + Min.Y);
        }

        public void FillWithTestPattern(string pattern)
        {
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    double X = (x * (Max.X - Min.X)) / (SizeX - 1) + Min.X;
                    double Y = (y * (Max.Y - Min.Y)) / (SizeY - 1) + Min.Y;

                    var d = (X * X + Y * Y) / 1000.0;
                    AddPoint(x, y, (double)d);
                }
            }
        }

        public void AddPoint(int x, int y, double height)
        {
            Points[x, y] = height;

            if (height > MaxHeight)
                MaxHeight = height;
            if (height < MinHeight)
                MinHeight = height;
        }


        public GCodeFile ApplyHeightMap(GCodeFile file)
        {
            double segmentLength = Math.Min(GridX, GridY);

            var newToolPath = new List<GCodeCommand>();

            foreach (var command in file.Commands)
            {
                if (command is MCode)
                {
                    newToolPath.Add(command);
                    continue;
                }
                else
                {
                    GCodeMotion m = (GCodeMotion)command;

                    foreach (GCodeMotion subMotion in m.Split(segmentLength))
                    {
                        subMotion.Start.Z += InterpolateZ(subMotion.Start.X, subMotion.Start.Y);
                        subMotion.End.Z += InterpolateZ(subMotion.End.X, subMotion.End.Y);

                        newToolPath.Add(subMotion);
                    }
                }
            }

            return GCodeFile.FromCommands(newToolPath);
        }

    }
}