using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Models
{
    public class PartPackCarrier
    {
        public PartPackCarrier()
        {
            AvailablePartPacks = new ObservableCollection<PartPackFeeder>();
            PartPackSlots = new ObservableCollection<PartPackSlot>();
        }

        public ObservableCollection<PartPackFeeder> AvailablePartPacks { get; set; }

        public ObservableCollection<PartPackSlot> PartPackSlots { get; set; }
    }
}
