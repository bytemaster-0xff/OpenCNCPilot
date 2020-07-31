using LagoVista.Core.Models;
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
        public PCB Board
        {
            get { return _board; }
            set { _board = value; }
        }

        public PnPJob()
        {
            Feeders = new ObservableCollection<FeederInstance>();
            Parts = new ObservableCollection<Part>();
        }

        private double _safeHeight;
        public double SafeHeight 
        {
            get => _safeHeight;
            set => Set(ref _safeHeight, value);
        }

        private double _boardHeight;
        public double BoardZ
        {
            get => _boardHeight;
            set => Set(ref _boardHeight, value);
        }

        private string _packagesPath;
        public string PackagesPath
        {
            get => _packagesPath;
            set => Set(ref _packagesPath, value);
        }

        public bool DispensePaste { get; set; }

        private string _eagleBRDFilePath;
        public string EagleBRDFilePath
        {
            get { return _eagleBRDFilePath; }
            set { Set(ref _eagleBRDFilePath, value); }
        }

        public ObservableCollection<Part> Parts { get; set; }

        public ObservableCollection<FeederInstance> Feeders { get; set; }

        public Task OpenAsync()
        {
            var doc = XDocument.Load(EagleBRDFilePath);

            _board = EagleParser.ReadPCB(doc);

            return Task.FromResult(default(object));
        }
    }
}
