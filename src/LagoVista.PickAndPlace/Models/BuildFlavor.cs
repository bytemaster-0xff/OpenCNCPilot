﻿using LagoVista.Core.Models;
using LagoVista.EaglePCB.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.Models
{
    public class BuildFlavor : ModelBase
    {
        string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        string _notes;
        public string Notes
        {
            get => _notes;
            set => Set(ref _notes, value);
        }

        public ObservableCollection<Component> Components { get; private set; } = new ObservableCollection<Component>();
    }
}
