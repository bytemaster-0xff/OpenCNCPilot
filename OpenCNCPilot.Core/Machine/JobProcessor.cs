using LagoVista.Core.GCode;
using LagoVista.Core.GCode.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LagoVista.GCode.Sender.Models;

namespace LagoVista.GCode.Sender
{
    public class JobProcessor : Core.Models.ModelBase, IJobProcessor
    {
        public IMachine _machine;
        GCodeFile _file;

        private bool _isDirty;

        private int _tail = 0;
        private int _head = 0;

        DateTime? _started;

        public int CurrentIndex { get { return _head; } }
        public int TotalLines { get { return _file.Commands.Count; } }

        public JobProcessor(IMachine machine, GCodeFile file)
        {
            _machine = machine;
            _file = file;
        }

        public void Process()
        {
            if (_started == null)
                _started = DateTime.Now;

            while (_machine.BufferSpaceAvailable(_file.Commands[Head].MessageLength) &&
                        Head < _file.Commands.Count)
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
            _file.Commands[Tail].StartTimeStamp = DateTime.Now;

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

        public TimeSpan TimeRemaining
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
    }
}
