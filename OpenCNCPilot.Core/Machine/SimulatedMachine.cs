using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender
{
    public class SimulatedMachine : ISerialPort
    {
        FirmwareTypes _firmwareType;

        public SimulatedMachine(FirmwareTypes firmwareType)
        {
            _firmwareType = firmwareType;
        }

        public bool IsConnected
        {
            get; set;
        }

        public Task CloseAsync()
        {
            IsConnected = false;
            return Task.FromResult(default(object));
        }

        public void Dispose()
        {
            
        }

        public Task<Stream> OpenAsync()
        {
            IsConnected = true;
            return Task.FromResult((new SimulatedGCodeMachine(_firmwareType) as Stream));
        }
    }

    public class SimulatedGCodeMachine : Stream
    {
        private List<byte> _outputArray = new List<byte>();

        FirmwareTypes _firmwareType;

        public SimulatedGCodeMachine(FirmwareTypes firmwareType)
        {
            _firmwareType = firmwareType;
        }

        public override bool CanRead { get { return true; } }

        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return true; } }

        public override long Length
        { 
            get { return _outputArray.Count; }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Flush()
        {
            
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            while(_outputArray.Count == 0)
            {
                System.Threading.Tasks.Task.Delay(10).Wait();
            }

            lock (_outputArray)
            {
                var bytesToCopy = Math.Min(count, _outputArray.Count);

                if (_outputArray.Count > 0)
                {
                    _outputArray.CopyTo(0, buffer, 0, bytesToCopy);
                }

                for (var idx = 0; idx < bytesToCopy; ++idx)
                {
                    _outputArray.RemoveAt(0);
                }

                return bytesToCopy;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        private void AddResponse(String response)
        {
            lock (_outputArray)
            {
                foreach (var ch in response)
                    _outputArray.Add((byte)ch);

                _outputArray.Add((byte)'\n');
            }
        }

        double GetParamValue(String param)
        {   double value  = 0;
            Double.TryParse(param.Substring(1), out value);
      
            return value;
        }

        private void HandleGCode(String cmd)
        {
            System.Threading.Tasks.Task.Delay(500).Wait();

            var parts = cmd.Split(' ');
            foreach(var part in parts.Where(itm=>!String.IsNullOrEmpty(itm)))
            {
                switch(part.Substring(0,1))
                {
                    case "X":
                        _machine.X = GetParamValue(part);
                        _work.X = GetParamValue(part);
                        break;
                    case "Y":
                        _machine.Y = GetParamValue(part);
                        _work.Y = GetParamValue(part);
                        break;
                    case "Z":
                        _machine.Z = GetParamValue(part);
                        _work.Z = GetParamValue(part);
                        break;
                }
            }
            AddResponse("ok");
        }

        private void HandleMCode(string cmd)
        {
            AddResponse("ok");
        }

        Vector3 _machine = new Vector3();
        Vector3 _work = new Vector3();

        private enum States
        {
            Idle,
            Run,
            Hold,
            Home,
            Alarm,
            Check,
            Door
        }

        States _state = States.Idle;


        private void HandleCommand(String command)
        {
            Debug.WriteLine(DateTime.Now.ToString() + "Handling command: " + command);
            var cmdLetter = command.First();
            switch(cmdLetter)
            {
                case '?':
                    AddResponse($"<{_state},MPos:{_machine.X},{_machine.Y},{_machine.Z},WPos:{_work.X},{_work.Y},{_work.Z}>");
                    break;
                case '$': AddResponse("ok"); break;
                case 'G': HandleGCode(command);
                    break;
                case 'M': HandleMCode(command);
                    System.Threading.Tasks.Task.Delay(100).Wait();
                    break;

            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var text = System.Text.Encoding.UTF8.GetString(buffer, offset, count);
            var commands = text.Split('\n');
            foreach(var command in commands.Where(cmd=> !String.IsNullOrEmpty(cmd)))
            {
                HandleCommand(command);
            }
        }
    }
}
