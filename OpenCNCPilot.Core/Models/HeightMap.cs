using System;
using System.Linq;
using System.Collections.Generic;
using LagoVista.Core.GCode;
using LagoVista.Core.GCode.Commands;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.Core.PlatformSupport;

namespace LagoVista.GCode.Sender.Models
{
    public partial class HeightMap : Core.Models.ModelBase
    {
        IMachine _machine;
        ILogger _logger;
        private String _fileName = null;

        public HeightMap(IMachine machine, ILogger logger)
        {
            RawBoardOutline = new System.Collections.ObjectModel.ObservableCollection<Line3D>();
            Points = new System.Collections.ObjectModel.ObservableCollection<HeightMapProbePoint>();

            _machine = machine;
            _logger = logger;
            _min = new Vector2(0, 0);
            _max = new Vector2(100, 80);
            _gridSize = 10;
            Refresh();
        }

        public void Refresh()
        {
            Points.Clear();
            RawBoardOutline.Clear();

            RaisePropertyChanged(nameof(Points));
            RaisePropertyChanged(nameof(RawBoardOutline));

            var min = Min;
            var max = Max;
            var gridSize = GridSize;
            if (min.X == max.X)
            {
                _machine.AddStatusMessage(StatusMessageTypes.Warning, $"Min X must not equal Max X, both are {min.X}");
                _logger.Log(LogLevel.Warning, "HeightMap_Refresh", $"Min X must not equal Max X, both are {min.X}");
                return;
            }

            if (min.Y == max.Y)
            {
                _machine.AddStatusMessage(StatusMessageTypes.Warning, $"Min Y must not equal Max Y, both are {min.Y}");
                _logger.Log(LogLevel.Warning, "HeightMap_Refresh", $"Min Y must not equal Max Y, both are {min.Y}");
                return;
            }

            if (min.X > max.X)
            {
                _machine.AddStatusMessage(StatusMessageTypes.Warning, $"Min X [{min.X}] must be greater than Max X [{max.X}]");
                _logger.Log(LogLevel.Warning, "HeightMap_Refresh", $"Min X [{min.X}] must be greater than Max X [{max.X}]");
                return;
            }

            if (min.Y > max.Y)
            {
                _machine.AddStatusMessage(StatusMessageTypes.Warning, $"Min Y [{min.Y}] must be greater than Max Y [{max.Y}]");
                _logger.Log(LogLevel.Warning, "HeightMap_Refresh", $"Min Y [{min.Y}] must be greater than Max Y [{max.Y}]");
                return;
            }

            if (gridSize == 0)
            {
                _machine.AddStatusMessage(StatusMessageTypes.Warning, $"Grid Size must not equal to 0.");
                _logger.Log(LogLevel.Warning, "HeightMap_Refresh", $"Grid Size must not be equal to 0.");
                return;
            }

            var pointsX = (int)Math.Ceiling((max.X - min.X) / gridSize) + 1;
            var pointsY = (int)Math.Ceiling((max.Y - min.Y) / gridSize) + 1;

            if (pointsX == 0 || pointsY == 0)
            {
                _machine.AddStatusMessage(StatusMessageTypes.Warning, $"Grid Size too large for board size.");
                _logger.Log(LogLevel.Warning, "HeightMap_Refresh", $"Grid Size too large for board size..");
                return;
            }         

            SizeX = pointsX;
            SizeY = pointsY;

            for (var x = 0; x < SizeX; x++)
            {
                for (var y = 0; y < SizeY; y++)
                {
                    var xPosition = (x * (Max.X - Min.X)) / (SizeX - 1) + Min.X;
                    var yPosition = (y * (Max.Y - Min.Y)) / (SizeY - 1) + Min.Y;

                    Points.Add(new HeightMapProbePoint() { XIndex = x, YIndex = y, Point = new Vector3(xPosition, yPosition, 0), Status = HeightMapProbePointStatus.NotProbed });
                }
            }

            RawBoardOutline.Add(Line3D.Create(Min.X, Min.Y, 0, Min.X, Max.Y, 0));
            RawBoardOutline.Add(Line3D.Create(Min.X, Max.Y, 0, Max.X, Max.Y, 0));
            RawBoardOutline.Add(Line3D.Create(Max.X, Max.Y, 0, Max.X, Min.Y, 0));
            RawBoardOutline.Add(Line3D.Create(Max.X, Min.Y, 0, Min.X, Min.Y, 0));

            RaisePropertyChanged(nameof(RawBoardOutline));
            RaisePropertyChanged(nameof(Points));
            RaisePropertyChanged(nameof(Min));
            RaisePropertyChanged(nameof(Max));
        }

        public HeightMapProbePoint GetNextPoint()
        {
            return Points.Where(pnt => pnt.Status == HeightMapProbePointStatus.NotProbed).FirstOrDefault();
        }

        public void FillWithTestPattern()
        {
            Points.Clear();
            RawBoardOutline.Clear();

            for (var x = 0; x < SizeX; x++)
            {
                for (var y = 0; y < SizeY; y++)
                {
                    var yPosition = (x * (Max.X - Min.X)) / (SizeX - 1) + Min.X;
                    var xPosition = (y * (Max.Y - Min.Y)) / (SizeY - 1) + Min.Y;

                    var height = (xPosition * xPosition + yPosition * yPosition) / 1000.0;
                    Points.Add(new HeightMapProbePoint() { XIndex = x, YIndex = y, Point = new Vector3(xPosition, yPosition, height), Status = HeightMapProbePointStatus.Probed });

                    MaxHeight = Math.Max(height, MaxHeight);
                    MinHeight = Math.Min(height, MinHeight);
                }
            }

            RawBoardOutline.Add(Line3D.Create(Min.X, Min.Y, 0, Min.X, Max.Y, 0));
            RawBoardOutline.Add(Line3D.Create(Min.X, Max.Y, 0, Max.X, Max.Y, 0));
            RawBoardOutline.Add(Line3D.Create(Max.X, Max.Y, 0, Max.X, Min.Y, 0));
            RawBoardOutline.Add(Line3D.Create(Max.X, Min.Y, 0, Min.X, Min.Y, 0));

            RaisePropertyChanged(nameof(RawBoardOutline));
            RaisePropertyChanged(nameof(Points));
            RaisePropertyChanged(nameof(Min));
            RaisePropertyChanged(nameof(Max));
            RaisePropertyChanged(nameof(MinHeight));
            RaisePropertyChanged(nameof(MaxHeight));
        }

        public void SetPointHeight(HeightMapProbePoint point, double height)
        {
            point.Point = new Vector3(point.Point.X, point.Point.Y, height);
            point.Status = HeightMapProbePointStatus.Probed;
            Status = Completed ? HeightMapStatus.Populated : HeightMapStatus.Populating;
            MaxHeight = Math.Max(height, MaxHeight);
            MinHeight = Math.Min(height, MinHeight);
            RaisePropertyChanged(nameof(MinHeight));
            RaisePropertyChanged(nameof(MaxHeight));
            RaisePropertyChanged(nameof(Points));
        }
    }
}