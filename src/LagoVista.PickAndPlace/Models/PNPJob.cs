using LagoVista.Core.Models;
using LagoVista.EaglePCB.Managers;
using LagoVista.EaglePCB.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
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
            BuildFlavors = new ObservableCollection<BuildFlavor>();
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

        private string _pnpMachinePath;
        public string PnPMachinePath
        {
            get => _pnpMachinePath;
            set => Set(ref _pnpMachinePath, value);
        }

        public bool DispensePaste { get; set; }

        private string _eagleBRDFilePath;
        public string EagleBRDFilePath
        {
            get { return _eagleBRDFilePath; }
            set { Set(ref _eagleBRDFilePath, value); }
        }

        public ObservableCollection<BuildFlavor> BuildFlavors { get; set; }

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
