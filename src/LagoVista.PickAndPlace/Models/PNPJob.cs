using LagoVista.Core.Models;
using LagoVista.EaglePCB.Managers;
using LagoVista.EaglePCB.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.PickAndPlace.Models
{
    public class PNPJob : ModelBase
    {
        PCB _board;
        [JsonIgnore]
        public PCB Board { get { return _board; } }

        private string _eagleBRDFilePath;
        public string EagleBRDFilePath
        {
            get { return _eagleBRDFilePath; }
            set { Set(ref _eagleBRDFilePath, value); }
        }

        public  Task OpenAsync()
        {
            var doc = XDocument.Load(EagleBRDFilePath);

            _board = EagleParser.ReadPCB(doc);

            return Task.FromResult(default(object));
        }
    }
}
