using LagoVista.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Models
{
    public class PartPackSlot : ModelBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        private double _x;
        public double X
        {
            get => _x;
            set => Set(ref _x, value);
        }

        private double _y;
        public double Y
        {
            get => _y;
            set => Set(ref _y, value);
        }

        private double _height;
        public double Height
        {
            get => _height;
            set => Set(ref _height, value);
        }

        private double _width;
        public double Width
        {
            get => _width;
            set => Set(ref _width, value);
        }

        private int _row;
        public int Row
        {
            get { return _row; }
            set { Set(ref _row, value); }
        }

        private int _column;
        public int Column
        {
            get { return _column; }
            set { Set(ref _column, value); }
        }

        private EntityHeader _partPack;
        public EntityHeader PartPack
        {
            get { return _partPack; }
            set { Set(ref _partPack, value); }
        }
    }
}
