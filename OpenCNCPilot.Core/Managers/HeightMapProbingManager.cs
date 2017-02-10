using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LagoVista.GCode.Sender.Models;
using LagoVista.GCode.Sender.Interfaces;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class HeightMapProbingManager : IHeightMapProbingManager
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ProbingCompleted;

        public event EventHandler ProbingCancelled;

        public void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public HeightMapProbingManager(IMachine machine)
        {
            Machine = machine;
        }

    }
}
