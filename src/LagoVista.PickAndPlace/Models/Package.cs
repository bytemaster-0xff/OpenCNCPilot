using LagoVista.Core.Models;
using System;
using LagoVista.Core;

namespace LagoVista.PickAndPlace.Models
{
    public class Package : ModelBase
    {
        public Package()
        {
            Id = Guid.NewGuid().ToId();
            HoleSpacing = 4;
            CenterXFromHole = 2;
            CenterYFromHole = 3.5;
            SpacingX = 4;
            CenterHoleFromBottom = 2.0;
            TapeWidth = 8;
        }

        public string Id { get; set; }

        public string Name { get; set; }
        
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        private double _centerXFromHole;
        public double CenterXFromHole
        {
            get => _centerXFromHole;
            set => Set(ref _centerXFromHole, value);
        }

        private double _centerHoleFromBottom;
        public double CenterHoleFromBottom
        {
            get => _centerHoleFromBottom;
            set => Set(ref _centerHoleFromBottom, value);
        }

        private double _centerYFromHole;
        public double CenterYFromHole
        {
            get => _centerYFromHole;
            set => Set(ref _centerYFromHole, value);
        }

        private double _holeSpacing;
        public double HoleSpacing
        {
            get => _holeSpacing;
            set => Set(ref _holeSpacing, value);
        }

        private double _spacingX;
        public double SpacingX
        {
            get => _spacingX;
            set => Set(ref _spacingX, value);
        }

        private double _tapeWidth;
        public double TapeWidth 
        {
            get => _tapeWidth;
            set => Set(ref _tapeWidth, value);
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set => Set(ref _notes, value);
        }

        public int RotationInTape { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
