using System.Collections.ObjectModel;

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
