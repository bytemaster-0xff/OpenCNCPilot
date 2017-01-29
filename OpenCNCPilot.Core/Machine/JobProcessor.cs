using LagoVista.Core.GCode;
using LagoVista.Core.GCode.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender
{
    public class JobProcessor
    {
        public IMachine _machine;
        StreamWriter _writer;
        GCodeFile _file;

        private int _tail = 0;
        private int _head = 0;

        public JobProcessor(IMachine machine, StreamWriter writer, GCodeFile file)
        {
            _machine = machine;
            _file = file;
            _writer = writer;
        }

        public void Process()
        {
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
    }
}
