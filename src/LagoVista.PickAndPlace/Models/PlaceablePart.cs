using LagoVista.EaglePCB.Models;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.Models
{
    public class PlaceableParts
    {
        public int Count { get; set; }

        public ObservableCollection<Component> Parts {get; set;}

        public int Available { get; set; }

        public string Package { get; set; }
        public string Value { get; set; }

        public PartPackSlot Slot { get; set; }
        public PartPackFeeder PartPack { get; set; }
        public Row Row { get; set; }
    }
}
