﻿using LagoVista.EaglePCB.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Interfaces
{
    public interface IPCBManager : INotifyPropertyChanged
    {
        bool HasBoard { get; }

        PCB Board { get; }

        Task<bool> OpenFileAsync(string boardFile);


        Task<bool> OpenProjectAsync(string projectFile);


        bool HasProject { get; }

        PCBProject Project { get; set; }

        String ProjectFilePath { get; set; }
    }
}
