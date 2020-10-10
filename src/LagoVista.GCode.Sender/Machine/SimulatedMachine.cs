using LagoVista.Core.GCode.Commands;
using LagoVista.Core.GCode.Parser;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender
{
    public class SimulatedMachine : ISerialPort
    {
        FirmwareTypes _firmwareType;
        Stream _simulatedStream;

        public SimulatedMachine(FirmwareTypes firmwareType)
        {
            _firmwareType = firmwareType;
        }

        public Stream InputStream { get { return _simulatedStream; } }

        public bool IsConnected
        {
            get; set;
        }

        public Stream OutputStream { get { return _simulatedStream; } }

        public Task CloseAsync()
        {
            IsConnected = false;
            return Task.FromResult(default(object));
        }

        public void Dispose()
        {

        }

        public Task OpenAsync()
        {
            IsConnected = true;
            _simulatedStream = (new SimulatedGCodeMachine(_firmwareType) as Stream);
            return Task.FromResult(default(object));
        }
    }

    public class SimulatedGCodeMachine : Stream
    {
        private List<byte> _outputArray = new List<byte>();
        GCodeParser _parser = new GCodeParser(Core.PlatformSupport.Services.Logger);
        Queue<String> _commands = new Queue<string>();
        ITimer _timer;
        FirmwareTypes _firmwareType;

        public SimulatedGCodeMachine(FirmwareTypes firmwareType)
        {
            _firmwareType = firmwareType;
            _timer = Services.TimerFactory.Create(TimeSpan.FromSeconds(0.5));
            _timer.Tick += _timer_Tick;

            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (_commands.Any())
            {
                _timer.Stop();
                while (_commands.Any())
                {
                    var cmd = _commands.Dequeue();
                    Debug.WriteLine("Start -> " + cmd);
                    HandleCommand(cmd);
                    Debug.WriteLine("End -> " + cmd);
                }
                _timer.Start();
            }
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

        SpinWait _bufferEmptySpinWait = new SpinWait();

        public override int Read(byte[] buffer, int offset, int count)
        {
            while (_outputArray.Count == 0)
            {
                _bufferEmptySpinWait.SpinOnce();
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
        {
            double value = 0;
            Double.TryParse(param.Substring(1), out value);

            return value;
        }

        int idx = 1;

        private void WriteOKResponse(String cmd)
        {
            if (_firmwareType == FirmwareTypes.LagoVista_PnP)
            {
                AddResponse($"<ok:{cmd}>");
            }
            else
            {
                AddResponse("ok - " + cmd);
            }
        }

        private void WriteOKResponse(GCodeCommand cmd)
        {
            WriteOKResponse(cmd.Command);
        }

        private void HandleGCode(String cmd)
        {
            Debug.WriteLine(DateTime.Now.ToString() + "  Handling G command: " + cmd);

            switch (cmd)
            {
                case "G91": WriteOKResponse(cmd); return;
                case "G90": WriteOKResponse(cmd); return;
                case "G21": WriteOKResponse(cmd); return;
                case "G20": WriteOKResponse(cmd); break;
            }

            if (cmd.StartsWith("G10"))
            {
                var parts = cmd.Split(' ');
                foreach (var part in parts)
                {
                    if (part.StartsWith("X")) _work.X = _machine.X;
                    if (part.StartsWith("Y")) _work.X = _machine.X;
                    if (part.StartsWith("Z")) _work.X = _machine.X;
                }

                return;
            }

            var line = _parser.CleanupLine(cmd, idx);
            var parsedLine = _parser.ParseMotionLine(line, idx);
            if (parsedLine == null)
            {

                WriteOKResponse(parsedLine);
                return;
            }

            if(parsedLine.Command == "G4")
            {
                System.Threading.Tasks.Task.Delay(250).Wait();

                WriteOKResponse("G4");
            }

            if (parsedLine.Command == "G0" ||
                parsedLine.Command == "G1" ||
                parsedLine.Command == "G00" ||
                parsedLine.Command == "G01")
            {
                Debug.WriteLine("Pausing For: " + parsedLine.EstimatedRunTime.ToString());
                var finishTime =  DateTime.Now + (_firmwareType == FirmwareTypes.LagoVista_PnP ? TimeSpan.FromSeconds(1) : parsedLine.EstimatedRunTime);
                SpinWait.SpinUntil(() => DateTime.Now > finishTime);

                var parts = cmd.Split(' ');
                foreach (var part in parts.Where(itm => !String.IsNullOrEmpty(itm)))
                {
                    switch (part.Substring(0, 1))
                    {
                        case "X":
                            _machine.X = GetParamValue(part);
                            break;
                        case "Y":
                            _machine.Y = GetParamValue(part);
                            break;
                        case "Z":
                            _machine.Z = GetParamValue(part);
                            break;
                        case "C":
                            _rotation = GetParamValue(part);
                            break;
                    }
                }

                WriteOKResponse(parsedLine);
            }
            else if (parsedLine.Command.StartsWith("G04"))
            {
                var finishTime = DateTime.Now + TimeSpan.FromSeconds(3);
                Debug.WriteLine("Handing Pause until " + finishTime + " or " + 3 + " seconds");
                SpinWait.SpinUntil(() => DateTime.Now > finishTime);
                WriteOKResponse(parsedLine);
                Debug.WriteLine("Done " + DateTime.Now);
            }
            else if (parsedLine.Command.StartsWith("G38"))
            {
                var finishTime = DateTime.Now + TimeSpan.FromSeconds(3);
                SpinWait.SpinUntil(() => DateTime.Now > finishTime);
                var newHeight = _rnd.NextDouble() - 0.5;

                AddResponse($"[PRB:0.00,0.00,{newHeight}:1]");
                WriteOKResponse(parsedLine);
            }
            else if (parsedLine.Command.StartsWith("G92"))
            {
                WriteOKResponse(parsedLine);
            }
        }

        Random _rnd = new Random(DateTime.Now.Millisecond);

        private void HandleMCode(string cmd)
        {
            Debug.WriteLine(DateTime.Now.ToString() + "  Handling M command: " + cmd);

            var cmdPair = cmd.Split(' ');

            switch(cmdPair[0])
            {
                case "M114":
                    var response = $"X: {_work.X} Y: {_work.Y} RZ: {_work.Z} LZ: 0.00 Count X:{_machine.X} Y: {_machine.Y} RZ: {_machine.Z} LZ: 41.02";
                    AddResponse(response);
                    WriteOKResponse(cmd);
                    break;
                case "M54":
                    _machine.Z = 10;
                    WriteOKResponse(cmd);
                    break;
                case "M55":
                    _machine.Z = 20;
                    WriteOKResponse(cmd);
                    break;
                case "M56":
                    _machine.Z = 30;
                    WriteOKResponse(cmd);
                    break;
                default:
                    WriteOKResponse(cmdPair[0]);
                    break;
            }
        }

        Vector3 _machine = new Vector3();
        Vector3 _work = new Vector3();
        double _rotation;

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
            var cmdLetter = command.First();
            switch (cmdLetter)
            {
                case '$': AddResponse("Status: OK"); break;
                case 'G':
                    HandleGCode(command);
                    break;
                case 'M':
                    HandleMCode(command);
                    System.Threading.Tasks.Task.Delay(100).Wait();
                    break;
                case 'S':
                    Debug.WriteLine("Setting Spindle Speed");
                    WriteOKResponse("S"); 

                    break;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var text = System.Text.Encoding.UTF8.GetString(buffer, offset, count);
            var commands = text.Split('\n');
            foreach (var command in commands.Where(cmd => !String.IsNullOrEmpty(cmd)))
            {
                var cleanCommand = command.Trim('\r');

                if (cleanCommand == "?")
                {
                    if (this._firmwareType == FirmwareTypes.GRBL1_1)
                    {
                        AddResponse($"<{_state},MPos:{_machine.X},{_machine.Y},{_machine.Z},WPos:{_work.X},{_work.Y},{_work.Z}>");
                    }
                    else
                    {
                        AddResponse($"<{_state},m:{_machine.X},{_machine.Y},{_machine.Z},{_machine.Z},{_rotation},0,camera>");

                    }
                }
                else
                {
                    _commands.Enqueue(command.TrimEnd('\r'));
                }
            }
        }
    }
}
