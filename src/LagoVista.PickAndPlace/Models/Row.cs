using LagoVista.Core.Models;

namespace LagoVista.PickAndPlace.Models
{
    public class Row : ModelBase
    {
        public int RowNumber { get; set; }

        private Part _part;
        public Part Part
        {
            get { return _part; }
            set { Set(ref _part, value); }
        }

        private double _firstComponentX;
        public double FirstComponentX 
        { 
            get { return _firstComponentX; }
            set { Set(ref _firstComponentX, value); }
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

        public double CurrentPartX
        {
            get
            {
                return _firstComponentX + (SpacingX * CurrentPartIndex);
            }
        }

        public string Display
        {
            get
            {
                if (Part != null)
                {
                    return $"{RowNumber}. {Part.Display} - {CurrentPartIndex}/{PartCount} - {CurrentPartX}";
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
