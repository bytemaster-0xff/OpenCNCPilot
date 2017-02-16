using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Interfaces
{
    public interface IProbingManager : INotifyPropertyChanged
    {
        RelayCommand BeginProbeCommand { get; }
        RelayCommand CancelProbeCommand { get; }

        Settings Settings {get;}

        void ProbeCompleted(string line);
    }
}
