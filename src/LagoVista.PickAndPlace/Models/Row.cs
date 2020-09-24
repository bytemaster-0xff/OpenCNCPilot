using LagoVista.Core.Models;
using Newtonsoft.Json;

namespace LagoVista.PickAndPlace.Models
{
    public class Row : ModelBase
    {
        public Row()
        {
            Part = new Part();
        }

        public int RowNumber { get; set; }

        private Part _part;
        public Part Part
        {
            get { return _part; }
            set { Set(ref _part, value); }
        }

        public double SpacingX { get; set; }

        private int _currentPartIndex;
        public int CurrentPartIndex 
        {
            get { return _currentPartIndex; }
            set { Set(ref _currentPartIndex, value); } 
        }

        private int _partCount;
        public int PartCount 
        { 
            get { return _partCount;  }
            set { Set(ref _partCount, value); }
        }

        [JsonIgnore]
        public int AvailableParts
        {
            get { return _partCount - _currentPartIndex; }
        }

        public string Display
        {
            get
            {
                if (Part != null)
                {
                    return $"{RowNumber}. {Part.Display} - {CurrentPartIndex + 1}/{PartCount}";
                }
                else
                {
                    return $"{RowNumber}.";
                }
            }
        }

        public override string ToString()
        {
            return Display;
        }

    }
}
