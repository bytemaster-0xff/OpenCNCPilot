using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Interfaces
{
    public interface IProbingManager
    {
        RelayCommand BeginProbeCommand { get; }
        RelayCommand CancelProbeCommand { get; }

        Settings Settings {get; set;}

        void ProbeCompleted(string line);
    }
}
