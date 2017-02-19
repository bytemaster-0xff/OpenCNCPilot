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
    public partial class GCodeFileManager : Core.Models.ModelBase, IGCodeFileManager
    {
        IMachine _machine;
        ILogger _logger;
        IToolChangeManager _toolChangeManager;

        GCodeFile _file;

        bool _isDirty;

        int _tail = 0;
        int _head = 0;

        int? _pendingToolChangeLine = null;

        DateTime? _started;

        public GCodeFileManager(IMachine machine, ILogger logger, IToolChangeManager toolChangeManager)
        {
            _machine = machine;
            _logger = logger;
            _toolChangeManager = toolChangeManager;

            Lines = new System.Collections.ObjectModel.ObservableCollection<Line3D>();
            RapidMoves = new System.Collections.ObjectModel.ObservableCollection<Line3D>();
            Arcs = new System.Collections.ObjectModel.ObservableCollection<Line3D>();

            HasValidFile = false;
        }

        private async void HandleToolChange(MCode mcode)
        {
            await _toolChangeManager.HandleToolChange(mcode);
            _pendingToolChangeLine = null;
            Tail++;
        }

        public void ProcessNextLines()
        {
            if (_started == null)
                _started = DateTime.Now;

            /* If we have queued up a pending tool change, don't send any more lines until tool change completed */
            if (_pendingToolChangeLine != null)
            {
                return;
            }

            while (Head < _file.Commands.Count &&
                _machine.HasBufferSpaceAvailableForByteCount(_file.Commands[Head].MessageLength))
            {
                /* If Next Command up is a Tool Change, set the nullable property to that line and bail. */
                if (Head  < _file.Commands.Count &&
                     (_file.Commands[Head].Command == "M06" || _file.Commands[Head].Command == "M6"))
                {
                    _pendingToolChangeLine = Head;
                    Head++;
                    return;
                }

                _machine.SendCommand(_file.Commands[Head]);
                _file.Commands[Head++].Status = GCodeCommand.StatusTypes.Sent;
            }
        }

        public GCodeCommand CurrentCommand
        {
            get { return _file == null ? null : _file.Commands[Tail]; }
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

            if (_pendingToolChangeLine != null && _pendingToolChangeLine.Value == Tail)
            {
                HandleToolChange(_file.Commands[Tail] as MCode);
            }

            return sentCommandLength;
        }
    }
}
