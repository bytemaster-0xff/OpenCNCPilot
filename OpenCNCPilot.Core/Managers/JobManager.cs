using LagoVista.Core.Commanding;
using LagoVista.Core.GCode;
using LagoVista.Core.GCode.Commands;
using LagoVista.Core.PlatformSupport;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.GCode.Sender.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class JobManager : Core.Models.ModelBase, IJobManager
    {
        IMachine _machine;
        ILogger _logger;
        IToolChangeManager _toolChangeManager;

        GCodeFile _file;

        bool _isDirty;

        int _tail = 0;
        int _head = 0;

        DateTime? _started;
       
        public JobManager(IMachine machine, ILogger logger, IToolChangeManager toolChangeManager)
        {
            _machine = machine;
            _logger = logger;
            _toolChangeManager = toolChangeManager;

            Lines = new System.Collections.ObjectModel.ObservableCollection<Line3D>();
            RapidMoves = new System.Collections.ObjectModel.ObservableCollection<Line3D>();
            Arcs = new System.Collections.ObjectModel.ObservableCollection<Line3D>();

            HasValidFile = false;
        }

        public void ProcessNextLines()
        {
            if (_started == null)
                _started = DateTime.Now;

            while (Head < _file.Commands.Count && 
                _machine.BufferSpaceAvailable(_file.Commands[Head].MessageLength))
            {
                _machine.SendCommand(_file.Commands[Head]);
                _file.Commands[Head++].Status = GCodeCommand.StatusTypes.Sent;
            }
        }

        public GCodeCommand CurrentCommand
        {
            get { return _file.Commands[Tail]; }
        }

        public int CommandAcknowledged()
        {
            var sentCommandLength = _file.Commands[Tail].MessageLength;
            _file.Commands[Tail++].Status = GCodeCommand.StatusTypes.Acknowledged;
            if (Tail < _file.Commands.Count)
            {
                _file.Commands[Tail].StartTimeStamp = DateTime.Now;
            }
            else
            {
                RaisePropertyChanged(nameof(IsCompleted));
            }

            return sentCommandLength;
        }
     }
}
