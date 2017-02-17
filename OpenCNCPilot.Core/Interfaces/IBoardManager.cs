using LagoVista.EaglePCB.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Interfaces
{
    public interface IBoardManager : INotifyPropertyChanged
    {
        bool HasBoard { get; }

        PCB Board { get; }

        Task LoadBoardAsync(string boardFile);
    }
}
