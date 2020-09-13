﻿using LagoVista.Core.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.Models
{
    public class PartPackFeeder : ModelBase
    {
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

                for (var idx = value - 1; idx < Rows.Count; ++idx)
                {
                    Rows.RemoveAt(idx);
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
