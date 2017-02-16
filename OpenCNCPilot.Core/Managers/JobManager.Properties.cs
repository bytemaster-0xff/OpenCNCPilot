using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class JobManager
    {
        public ObservableCollection<Line3D> Lines { get; set; }

        public ObservableCollection<Line3D> RapidMoves { get; set; }

        public ObservableCollection<Line3D> Arcs { get; set; }
    }

    public class Line3D
    {
        public LagoVista.Core.Models.Drawing.Vector3 Start { get; set; }
        public LagoVista.Core.Models.Drawing.Vector3 End { get; set; }

    }
}
