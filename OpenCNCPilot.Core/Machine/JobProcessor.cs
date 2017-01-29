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
    public class JobProcessor : IJobProcessor
    {
        public IMachine _machine;
        GCodeFile _file;

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

            while (_machine.BufferSpaceAvailable(_file.Commands[_head].MessageLength) && _head < _file.Commands.Count)
            {
                _machine.SendCommand(_file.Commands[_head++]);
            }
        }

        public GCodeCommand CurrentCommand
        {
            get { return _file.Commands[_tail]; }
        }

        public int CommandAcknowledged()
        {
            var sentCommandLength = _file.Commands[_tail].MessageLength;
            _tail++;
            _file.Commands[_tail].StartTimeStamp = DateTime.Now;

            return sentCommandLength;
        }

        public bool Completed
        {
            get { return _tail == _head; }
        }

        public void Reset()
        {

        }

        public void ApplyHeightMap(HeightMap map)
        {
            _file = map.ApplyHeightMap(_file);
        }

        public void ArcToLines(double length)
        {
            _file = _file.ArcsToLines(length);
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
                if(_started.HasValue)
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
                if(_started.HasValue)
                {
                    return _started.Value.Add(_file.EstimatedRunTime);
                }
                else
                {
                    return DateTime.Now;
                }
                
            }
        }

        public IEnumerable<GCodeCommand> Commands
        {
            get { return _file.Commands; }
        }
    }
}
