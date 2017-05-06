using LagoVista.Core.PlatformSupport;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public class ManagerBase
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ManagerBase(IMachine machine, ILogger logger)
        {
            Machine = machine;
            Logger = logger;
        }

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected IMachine Machine { get; private set; }
        protected ILogger Logger { get; private set; }
    }
}
