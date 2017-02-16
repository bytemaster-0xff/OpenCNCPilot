using LagoVista.GCode.Sender.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Interfaces
{
    public interface IHeightMapManager : INotifyPropertyChanged
    {
        event EventHandler ProbingCompleted;
        event EventHandler ProbingCancelled;

        HeightMap HeightMap { get; }

        bool HasHeightMap { get; }

        void ProbeCompleted(string line);

        void StartProbing();
        void PauseProbing();

        IMachine Machine { get; }
    }
}
