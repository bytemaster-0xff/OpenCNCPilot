using LagoVista.Core.Commanding;
using LagoVista.Core.PlatformSupport;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class ProbingManager
    {
        public ILogger Logger { get; private set; }

        public IMachine Machine { get; private set; }

        public Settings Settings { get; private set; }

        public RelayCommand BeginProbeCommand { get; private set; }
     
        public RelayCommand CancelProbeCommand { get; private set; }
    }
}
