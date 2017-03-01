using LagoVista.EaglePCB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class PCBManager
    {
        private PCB _board;
        public PCB Board
        {
            get { return _board; }
            set
            {
                _board = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasBoard));
            }
        }

        public bool HasBoard
        {
            get { return _board != null; }
        }

    }
}
