using LagoVista.Core.PlatformSupport;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LagoVista.Core.Commanding;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class ProbingManager : IProbingManager
    {
        

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ProbeCompleted(string line)
        {
            throw new NotImplementedException();
        }

        public ProbingManager(IMachine machine, ILogger logger, Settings settings)
        {
            Machine = machine;
            Logger = logger;
        }
    }
}
