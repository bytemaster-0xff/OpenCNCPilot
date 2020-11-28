using LagoVista.Core.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.Models
{
    public class PartPackFeeder : ModelBase
    {
        public PartPackFeeder()
        {
            Width = 70;
            Height = 70;
            Pin1XOffset = 7.0;
            Pin1YOffset = 6.5;
        }

        private string _id;
        public string Id
        {
            get => _id;
            set => Set(ref _id, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private double _partZ;
        public double PartZOffset
        {
            get => _partZ;
            set => Set(ref _partZ, value);
        }

        private double _pin1X;
        public double Pin1XOffset
        {
            get => _pin1X;
            set => Set(ref _pin1X, value);
        }

        private double _pin1Y;
        public double Pin1YOffset
        {
            get => _pin1Y;
            set => Set(ref _pin1Y, value);
        }

        private PartPackSlot _currentSlot;
        public PartPackSlot CurrentSlot
        {
            get => _currentSlot;
            set => Set(ref _currentSlot, value);
        }

        private double _pickZHeight;
        public double PickZHeight
        {
            get => _pickZHeight;
            set => Set(ref _pickZHeight, value);
        }

        private double _width;
        public double Width
        {
            get => _width;
            set => Set(ref _width, value);
        }

        private double _height;
        public double Height
        {
            get => _height;
            set => Set(ref _height, value);
        }

        private double _correctionAngleX;
        public double CorrectionAngleX
        {
            get => _correctionAngleX;
            set => Set(ref _correctionAngleX, value);
        }

        private double _correctionAngleY;
        public double CorrectionAngleY
        {
            get => _correctionAngleY;
            set => Set(ref _correctionAngleY, value);
        }

        public ObservableCollection<Row> Rows { get; set; } = new ObservableCollection<Row>();
        
        private double _rowHeight;
        public double RowHeight 
        {
            get => _rowHeight;
            set => Set(ref _rowHeight, value);
        }

        [JsonIgnore]
        public int RowCount
        {
            get => Rows.Count;
            set
            {
                for(var idx = Rows.Count; idx < value; ++idx)
                {
                    Rows.Add(new Row());
                }

                for (var idx = value; idx < Rows.Count; ++idx)
                {
                    Rows.RemoveAt(idx);
                }

                var rowNumber = 1; 
                foreach(var row in Rows)
                {
                    row.RowNumber = rowNumber++;
                }
            }
        }

        private Row _selectedRow = null;
        [JsonIgnore]
        public Row SelectedRow
        {
            get => _selectedRow;
            set => Set(ref _selectedRow, value);
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set => Set(ref _notes, value);
        }
    }
}
