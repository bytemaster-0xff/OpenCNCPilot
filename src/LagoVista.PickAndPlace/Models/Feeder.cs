using System;
using System.Collections.Generic;
using LagoVista.Core;
using LagoVista.Core.Models;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Defines the Physical Definition of a Parts Tray
    /// </summary>
    public class Feeder : ModelBase
    {
        public string Id { get; set; }

        public Feeder()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Name { get; set; }

        public double Width { get; set; }
        public double Length{ get; set; }
        public double PartZ { get; set; }

        public double TapeWidth { get; set; }

        public double RowWidth { get; set; }

        public double FirstRowOffset { get; set; }

        public bool IsStatic { get; set; }

        private int _numberRows = 1;
        public int NumberRows 
        { 
            get { return _numberRows; }
            set{Set(ref _numberRows, value);}
        }

        private double _x;
        private double _y;
        public double X 
        { 
            get { return _x; }
            set
            {
                Set(ref _x, value);
            }
        }
        public double Y
        {
            get { return _y; }
            set
            {
                Set(ref _y, value);
            }
        }

        private double _firstPartXOffset;
        public double FirstPartXOffset
        {
            get { return _firstPartXOffset; }
            set { Set(ref _firstPartXOffset, value); }
        }

        public List<double> RowYHoleOffsets { get; private set; } = new List<double>();

        public override string ToString()
        {
            return $"{Name} - {X} {Y}";
        }
    }
}
