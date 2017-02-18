using LagoVista.Core.Models.Drawing;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LagoVista.GCode.Sender.Models
{
    public enum HeightMapStatus
    {
        NotPopulated,
        Populating,
        Populated,
    }

    public partial class HeightMap
    {

        public int SizeX { get; set; }
        public int SizeY { get; set; }

        public int Progress { get { return Points.Count - Points.Where(prb => prb.Status == HeightMapProbePointStatus.Probed).Count(); } }

        public int TotalPoints { get { return Points.Count; } }

        public bool Completed { get { return Points.Where(prb => prb.Status == HeightMapProbePointStatus.Probed).Count() == Points.Count; } }

        private Vector2 _min;
        public Vector2 Min
        {
            get { return _min; }
            set
            {
                Set(ref _min, value);
                Refresh();
            }
        }

        private Vector2 _max;
        public Vector2 Max
        {
            get { return _max; }
            set
            {
                Set(ref _max, value);
                Refresh();
            }
        }

        private double _gridSize;
        public double GridSize
        {
            get { return _gridSize; }
            set
            {
                Set(ref _gridSize, value);
                Refresh();
            }
        }

        HeightMapStatus _status = HeightMapStatus.NotPopulated;
        public HeightMapStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                RaisePropertyChanged();
            }
        }

        public Vector2 Delta { get { return Max - Min; } }

        public double MinHeight { get; set; } = 0;
        public double MaxHeight { get; set; } = 0;

        public double GridX { get { return (Max.X - Min.X) / (SizeX - 1); } }
        public double GridY { get { return (Max.Y - Min.Y) / (SizeY - 1); } }

        public ObservableCollection<Line3D> RawBoardOutline { get; private set; }

        public ObservableCollection<HeightMapProbePoint> Points { get; private set; }
    }
}
