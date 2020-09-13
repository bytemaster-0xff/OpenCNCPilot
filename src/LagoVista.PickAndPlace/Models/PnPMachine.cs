using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Models
{
    public class PnPMachine
    {   
        public PnPMachine()
        {
            Carrier = new PartPackCarrier();
            Packages = new ObservableCollection<Package>();
        }

        public PartPackCarrier Carrier { get; set; }

        public ObservableCollection<Package> Packages { get; set; }
    }
}
