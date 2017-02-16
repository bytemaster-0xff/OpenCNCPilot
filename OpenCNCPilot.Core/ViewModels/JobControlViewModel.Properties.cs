using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class JobControlViewModel
    {
        public bool IsCreatingHeightMap { get { return Machine.Mode == OperatingMode.ProbingHeightMap; } }
        public bool IsProbingHeight { get { return Machine.Mode == OperatingMode.ProbingHeight; } }
        public bool IsRunningJob { get { return Machine.Mode == OperatingMode.SendingJob; } }

        public Managers.HeightMapManager HeightMapProbingManager { get; private set; }
    }
}
