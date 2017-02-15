using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.EaglePCB.Models
{
    public class PCB
    {
        public ObservableCollection<Drill> Drills { get; private set; }

    }
}
