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
        GCodeFile _file;

        bool _isDirty;

        int _tail = 0;
        int _head = 0;

        DateTime? _started;
       
        public JobManager(IMachine machine, ILogger logger)
        {
            _machine = machine;
            _logger = logger;

            Lines = new System.Collections.ObjectModel.ObservableCollection<Line3D>();
            RapidMoves = new System.Collections.ObjectModel.ObservableCollection<Line3D>();
            Arcs = new System.Collections.ObjectModel.ObservableCollection<Line3D>();

            HasValidFile = true;
        }


        public Task OpenFileAsync(string path)
        {
            _file = GCodeFile.Load(path);
            if (_file != null)
            {
                RenderPaths();
            }
            else
            {
                ClearPaths();
            }

            return Task.FromResult(default(object));
        }

        public void ProcessNextLines()
        {
            if (_started == null)
                _started = DateTime.Now;

            while (Head < _file.Commands.Count && _machine.BufferSpaceAvailable(_file.Commands[Head].MessageLength))
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

        public bool IsDirty
        {
            get { return _isDirty; }
        }

        public bool IsCompleted
        {
            get { return Tail == TotalLines; }
        }

        public void Reset()
        {
            Head = 0;
            Tail = 0;
            foreach (var cmd in Commands)
            {
                cmd.Status = GCodeCommand.StatusTypes.Ready;
            }
        }

        public void QueueAllItems()
        {
            foreach (var cmd in Commands)
            {
                cmd.Status = GCodeCommand.StatusTypes.Ready;
            }
        }

        public void ApplyHeightMap(HeightMap map)
        {
            _file = map.ApplyHeightMap(_file);
            _isDirty = true;
        }

        public void ArcToLines(double length)
        {
            _file = _file.ArcsToLines(length);
            _isDirty = true;
        }

        public Task OpenGCodeFileAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task CloseFileAsync()
        {
            throw new NotImplementedException();
        }

        public TimeSpan EstimatedTimeRemaining
        {
            get
            {
                return _file.EstimatedRunTime - ElapsedTime;
            }
        }

        public TimeSpan ElapsedTime
        {
            get
            {
                if (_started.HasValue)
                    return DateTime.Now - _started.Value;
                else
                {
                    return TimeSpan.Zero;
                }
            }
        }

        public DateTime EstimatedCompletion
        {
            get
            {
                if (_started.HasValue)
                {
                    return _started.Value.Add(_file.EstimatedRunTime);
                }
                else
                {
                    return DateTime.Now;
                }

            }
        }

        public int CurrentIndex { get { return _head; } }
        public int TotalLines { get { return _file.Commands.Count; } }


        public int Head
        {
            get { return _head; }
            set { Set(ref _head, value); }
        }

        public int Tail
        {
            get { return _tail; }
            set { Set(ref _tail, value); }
        }

        public IEnumerable<GCodeCommand> Commands
        {
            get { return _file.Commands; }
        }

        private bool _hasValidFile = false;
        public bool HasValidFile
        {
            get { return _hasValidFile; }
            set { Set(ref _hasValidFile, value); }
        }

        public RelayCommand StartJobCommand { get; private set; }

        public RelayCommand StopJobCommand { get; private set; }

        public RelayCommand PauseJobCommand { get; private set; }
        public RelayCommand ResetJobCommand { get; private set; }
    }
}
