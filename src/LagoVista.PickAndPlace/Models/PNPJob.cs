﻿using LagoVista.Core.Models;
using LagoVista.EaglePCB.Managers;
using LagoVista.EaglePCB.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.PickAndPlace.Models
{
    public class PnPJob : ModelBase
    {
        PCB _board;
        [JsonIgnore]
        public PCB Board { get { return _board; } }

        public PnPJob()
        {
            Feeders = new ObservableCollection<Feeder>();
            Parts = new ObservableCollection<Part>();
        }

        public string PackagesPath { get; set; }

        public bool DispensePaste { get; set; }

        private string _eagleBRDFilePath;
        public string EagleBRDFilePath
        {
            get { return _eagleBRDFilePath; }
            set { Set(ref _eagleBRDFilePath, value); }
        }

        public ObservableCollection<Part> Parts { get; set; }

        public ObservableCollection<Feeder> Feeders { get; set; }

        public  Task OpenAsync()
        {
            var doc = XDocument.Load(EagleBRDFilePath);

            _board = EagleParser.ReadPCB(doc);

            return Task.FromResult(default(object));
        }
    }
}