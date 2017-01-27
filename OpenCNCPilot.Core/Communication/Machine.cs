using OpenCNCPilot.Core.GCode;
using OpenCNCPilot.Core.GCode.GCodeCommands;
using OpenCNCPilot.Core.Platform;
using OpenCNCPilot.Core.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LagoVista.Core.PlatformSupport;

namespace OpenCNCPilot.Core.Communication
{
    enum ConnectionType
    {
        Serial
    }

    public class Machine : IMachine
    {
        public enum OperatingMode
        {
            Manual,
            SendFile,
            Probe,
            Disconnected
        }

        public event Action<Vector3, bool> ProbeFinished;
        public event Action<string> NonFatalException;
        public event Action<string> Info;
        public event Action<string> LineReceived;
        public event Action<string> LineSent;
        public event Action ConnectionStateChanged;
        public event Action PositionUpdateReceived;
        public event Action StatusChanged;
        public event Action DistanceModeChanged;
        public event Action UnitChanged;
        public event Action PlaneChanged;
        public event Action BufferStateChanged;
        public event Action OperatingModeChanged;
        public event Action FileChanged;
        public event Action FilePositionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public Machine(Settings settings)
        {
            _settings = settings;
        }

        public void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        Settings _settings;

        CancellationToken _cancelToken;

        private Vector3 _machinePosition = new Vector3();
        public Vector3 MachinePosition
        {
            get { return _machinePosition; }
            set
            {
                _machinePosition = value;
                RaisePropertyChanged();
            }
        }

        private Vector3 _workPosition = new Vector3();
        public Vector3 WorkPosition
        {
            get { return _workPosition; }
            set
            {
                _workPosition = value;
                RaisePropertyChanged();
            }
        }

        private ReadOnlyCollection<string> _file = new ReadOnlyCollection<string>(new string[0]);
        public ReadOnlyCollection<string> File
        {
            get { return _file; }
            set
            {
                _file = value;
                FilePosition = 0;
                RaiseEvent(FileChanged);
            }
        }

        private int _filePosition = 0;
        public int FilePosition
        {
            get { return _filePosition; }
            private set
            {
                _filePosition = value;
                RaiseEvent(FilePositionChanged);
            }
        }

        private OperatingMode _mode = OperatingMode.Disconnected;
        public OperatingMode Mode
        {
            get { return _mode; }
            private set
            {
                if (_mode == value)
                    return;

                _mode = value;
                RaiseEvent(OperatingModeChanged);

                RaisePropertyChanged();
            }
        }

        #region Status
        private string _status = "Disconnected";
        public string Status
        {
            get { return _status; }
            private set
            {
                if (_status == value)
                    return;
                _status = value;

                RaiseEvent(StatusChanged);

                RaisePropertyChanged();
            }
        }

        private ParseDistanceMode _distanceMode = ParseDistanceMode.Absolute;
        public ParseDistanceMode DistanceMode
        {
            get { return _distanceMode; }
            private set
            {
                if (_distanceMode == value)
                    return;
                _distanceMode = value;

                RaiseEvent(DistanceModeChanged);
            }
        }

        private ParseUnit _unit = ParseUnit.Metric;
        public ParseUnit Unit
        {
            get { return _unit; }
            private set
            {
                if (_unit == value)
                    return;
                _unit = value;

                RaiseEvent(UnitChanged);
            }
        }

        private ArcPlane _plane = ArcPlane.XY;
        public ArcPlane Plane
        {
            get { return _plane; }
            private set
            {
                if (_plane == value)
                    return;
                _plane = value;

                RaiseEvent(PlaneChanged);
            }
        }

        private bool _connected = false;
        public bool Connected
        {
            get { return _connected; }
            private set
            {
                if (value == _connected)
                    return;

                _connected = value;

                if (!Connected)
                    Mode = OperatingMode.Disconnected;

                RaiseEvent(ConnectionStateChanged);

                RaisePropertyChanged();
            }
        }

        private int _bufferState;
        public int BufferState
        {
            get { return _bufferState; }
            private set
            {
                if (_bufferState == value)
                    return;

                _bufferState = value;

                RaiseEvent(BufferStateChanged);
            }
        }
        #endregion Status


        Queue<string> _sentQueue = new Queue<string>();
        Queue<string> _toSend = new Queue<string>();
        Queue<string> _toSendPriority = new Queue<string>();

        private async void Work(Stream stream)
        {
            try
            {
                StreamReader reader = new StreamReader(stream);
                StreamWriter writer = new StreamWriter(stream);

                int StatusPollInterval = _settings.StatusPollInterval;

                int ControllerBufferSize = _settings.ControllerBufferSize;
                BufferState = 0;

                TimeSpan WaitTime = TimeSpan.FromMilliseconds(0.5);
                DateTime LastStatusPoll = DateTime.Now + TimeSpan.FromSeconds(0.5);
                DateTime StartTime = DateTime.Now;

                writer.Write("\n$G\n");
                writer.Flush();

                while (true)
                {
                    Task<string> lineTask = reader.ReadLineAsync();

                    while (!lineTask.IsCompleted)
                    {
                        if (!Connected)
                        {
                            return;
                        }

                        while (_toSendPriority.Count > 0)
                        {
                            writer.Write(_toSendPriority.Dequeue());
                            writer.Flush();
                        }
                        if (Mode == OperatingMode.SendFile)
                        {
                            if (File.Count > FilePosition && (File[FilePosition].Length + 1) < (ControllerBufferSize - BufferState))
                            {
                                string send_line = File[FilePosition++];

                                writer.Write(send_line);
                                writer.Write('\n');
                                writer.Flush();

                                RaiseEvent(UpdateStatus, send_line);
                                RaiseEvent(LineSent, send_line);

                                BufferState += send_line.Length + 1;

                                _sentQueue.Enqueue(send_line);

                                if (FilePosition >= File.Count)
                                {
                                    Mode = OperatingMode.Manual;
                                }

                                continue;
                            }
                        }
                        else
                        {
                            if (_toSend.Count > 0 && ((_toSend.Peek().ToString()).Length + 1) < (ControllerBufferSize - BufferState))
                            {
                                var send_line = _toSend.Peek();

                                writer.Write(send_line);
                                writer.Write('\n');
                                writer.Flush();

                                RaiseEvent(UpdateStatus, send_line.ToString());
                                RaiseEvent(LineSent, send_line.ToString());

                                BufferState += 1;

                                _sentQueue.Enqueue(send_line);
                                _toSend.Dequeue();

                                continue;
                            }
                        }

                        DateTime Now = DateTime.Now;

                        if ((Now - LastStatusPoll).TotalMilliseconds > StatusPollInterval)
                        {
                            writer.Write('?');
                            writer.Flush();
                            LastStatusPoll = Now;
                        }

                        await Task.Delay(WaitTime);
                    }

                    string line = lineTask.Result;
                    if (line == "ok")
                    {
                        if (_sentQueue.Count != 0)
                        {
                            BufferState -= ((string)_sentQueue.Dequeue()).Length + 1;
                        }
                        else
                        {

                            LagoVista.Core.PlatformSupport.Services.Logger.Log(LagoVista.Core.PlatformSupport.LogLevel.Warning, "Machine_Work", "Received OK without anything in the Sent Buffer");
                            BufferState = 0;
                        }
                    }
                    else
                    {
                        if (line.StartsWith("error: "))
                        {
                            if (_sentQueue.Count != 0)
                            {
                                var errorline = _sentQueue.Dequeue();

                                RaiseEvent(ReportError, $"{line}: {errorline}");
                                BufferState -= errorline.Length + 1;
                            }
                            else
                            {
                                if ((DateTime.Now - StartTime).TotalMilliseconds > 200)
                                    RaiseEvent(ReportError, $"Received <{line}> without anything in the Sent Buffer");

                                BufferState = 0;
                            }

                            Mode = OperatingMode.Manual;
                        }
                        else if (line.StartsWith("<"))
                            RaiseEvent(ParseStatus, line);
                        else if (line.StartsWith("[PRB:"))
                        {
                            RaiseEvent(ParseProbe, line);
                            RaiseEvent(LineReceived, line);
                        }
                        else if (line.StartsWith("["))
                        {
                            RaiseEvent(UpdateStatus, line);
                            RaiseEvent(LineReceived, line);
                        }
                        else if (line.StartsWith("ALARM"))
                        {
                            RaiseEvent(NonFatalException, line);
                            Mode = OperatingMode.Manual;
                        }
                        else if (line.Length > 0)
                            RaiseEvent(LineReceived, line);
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseEvent(ReportError, $"Fatal Error: {ex.Message}");
                Disconnect();
            }
        }

        public void Connect(Stream outputStream)
        {
            if (Connected)
                throw new Exception("Can't Connect: Already Connected");

            Connected = true;

            _toSend.Clear();
            _sentQueue.Clear();

            Mode = OperatingMode.Manual;

            if (PositionUpdateReceived != null)
                PositionUpdateReceived.Invoke();

            _cancelToken = new CancellationToken();

            Task.Run(() =>
            {
                Work(outputStream);
            }, _cancelToken);
        }

        public void Disconnect()
        {
            if (!Connected)
                throw new Exception("Can't Disconnect: Not Connected");


            Mode = OperatingMode.Disconnected;

            MachinePosition = new Vector3();
            WorkPosition = new Vector3();

            if (PositionUpdateReceived != null)
                PositionUpdateReceived.Invoke();

            Status = "Disconnected";
            DistanceMode = ParseDistanceMode.Absolute;
            Unit = ParseUnit.Metric;
            Plane = ArcPlane.XY;
            BufferState = 0;

            _toSend.Clear();
            _toSendPriority.Clear();
            _sentQueue.Clear();
        }

        public void SendLine(string line)
        {
            if (!Connected)
            {
                RaiseEvent(Info, "Not Connected");
                return;
            }

            if (Mode != OperatingMode.Manual && Mode != OperatingMode.Probe)
            {
                RaiseEvent(Info, "Not in Manual Mode");
                return;
            }

            lock (_toSend)
            {
                _toSend.Enqueue(line);
            }
        }

        public void SoftReset()
        {
            if (!Connected)
            {
                RaiseEvent(Info, "Not Connected");
                return;
            }

            Mode = OperatingMode.Manual;

            lock (_toSend)
                lock (_toSendPriority)
                    lock (_sentQueue)
                    {
                        _toSend.Clear();
                        _toSendPriority.Clear();
                        _sentQueue.Clear();
                        _toSendPriority.Enqueue(((char)0x18).ToString());
                    }

            BufferState = 0;
        }

        public void FeedHold()
        {
            if (!Connected)
            {
                RaiseEvent(Info, "Not Connected");
                return;
            }

            lock (_toSendPriority)
            {
                _toSendPriority.Enqueue("!");
            }
        }

        public void CycleStart()
        {
            if (!Connected)
            {
                RaiseEvent(Info, "Not Connected");
                return;
            }

            lock (_toSendPriority)
            {
                _toSendPriority.Enqueue("~");
            }
        }

        public void SetFile(IList<string> file)
        {
            if (Mode == OperatingMode.SendFile)
            {
                RaiseEvent(Info, "Can't change file while active");
                return;
            }

            File = new ReadOnlyCollection<string>(file);
            FilePosition = 0;
        }

        public void ClearFile()
        {
            if (Mode == OperatingMode.SendFile)
            {
                RaiseEvent(Info, "Can't change file while active");
                return;
            }

            File = new ReadOnlyCollection<string>(new string[0]);
            FilePosition = 0;
        }

        public void FileStart()
        {
            if (!Connected)
            {
                RaiseEvent(Info, "Not Connected");
                return;
            }

            if (Mode != OperatingMode.Manual)
            {
                RaiseEvent(Info, "Not in Manual Mode");
                return;
            }

            Mode = OperatingMode.SendFile;
        }

        public void FilePause()
        {
            if (!Connected)
            {
                RaiseEvent(Info, "Not Connected");
                return;
            }

            if (Mode != OperatingMode.SendFile)
            {
                RaiseEvent(Info, "Not in SendFile Mode");
                return;
            }

            Mode = OperatingMode.Manual;
        }

        public void ProbeStart()
        {
            if (!Connected)
            {
                RaiseEvent(Info, "Not Connected");
                return;
            }

            if (Mode != OperatingMode.Manual)
            {
                RaiseEvent(Info, "Can't start probing while running!");
                return;
            }

            Mode = OperatingMode.Probe;
        }

        public void ProbeStop()
        {
            if (!Connected)
            {
                RaiseEvent(Info, "Not Connected");
                return;
            }

            if (Mode != OperatingMode.Probe)
            {
                RaiseEvent(Info, "Not in Probe mode");
                return;
            }

            Mode = OperatingMode.Manual;
        }

        public void FileGoto(int lineNumber)
        {
            if (Mode == OperatingMode.SendFile)
                return;

            if (lineNumber >= File.Count || lineNumber < 0)
            {
                RaiseEvent(NonFatalException, "Line Number outside of file length");
                return;
            }

            FilePosition = lineNumber;
        }

        public void ClearQueue()
        {
            if (Mode != OperatingMode.Manual)
            {
                RaiseEvent(Info, "Not in Manual mode");
                return;
            }

            lock (_toSend)
            {
                _toSend.Clear();
            }
        }

        //TODO: Removed Compiled Option
        private static Regex GCodeSplitter = new Regex(@"(G)\s*(\-?\d+\.?\d*)");

        /// <summary>
        /// Updates Status info from each line sent
        /// </summary>
        /// <param name="line"></param>
        private void UpdateStatus(string line)
        {
            if (!Connected)
                return;

            //we use a Regex here so G91.1 etc don't get recognized as G91

            foreach (Match m in GCodeSplitter.Matches(line))
            {
                if (m.Groups[1].Value != "G")
                    continue;

                float code = float.Parse(m.Groups[2].Value);

                if (code == 17)
                    Plane = ArcPlane.XY;
                if (code == 18)
                    Plane = ArcPlane.YZ;
                if (code == 19)
                    Plane = ArcPlane.ZX;

                if (code == 20)
                    Unit = ParseUnit.Imperial;
                if (code == 21)
                    Unit = ParseUnit.Metric;

                if (code == 90)
                    DistanceMode = ParseDistanceMode.Absolute;
                if (code == 91)
                    DistanceMode = ParseDistanceMode.Incremental;
            }
        }

        //TODO: Removed compiled option
        private static Regex StatusEx = new Regex(@"<(?'State'Idle|Run|Hold|Home|Alarm|Check|Door)(?:.MPos:(?'MX'-?[0-9\.]*),(?'MY'-?[0-9\.]*),(?'MZ'-?[0-9\.]*))?(?:,WPos:(?'WX'-?[0-9\.]*),(?'WY'-?[0-9\.]*),(?'WZ'-?[0-9\.]*))?(?:,Buf:(?'Buf'[0-9]*))?(?:,RX:(?'RX'[0-9]*))?(?:,Ln:(?'L'[0-9]*))?(?:,F:(?'F'[0-9\.]*))?(?:,Lim:(?'Lim'[0-1]*))?(?:,Ctl:(?'Ctl'[0-1]*))?(?:.FS:(?'FSX'-?[0-9\.]*),(?'FSY'-?[0-9\.]*))?(?:.WCO:(?'WCOX'-?[0-9\.]*),(?'WCOY'-?[0-9\.]*),(?'WCOZ'-?[0-9\.]*))?(?:.Ov:(?'OVX'-?[0-9\.]*),(?'OVY'-?[0-9\.]*),(?'OVZ'-?[0-9\.]*))?>");

        /// <summary>
        /// Parses a recevied status report (answer to '?')
        /// </summary>
        private void ParseStatus(string line)
        {
            Match statusMatch = StatusEx.Match(line);

            if (!statusMatch.Success)
            {
                NonFatalException.Invoke(string.Format("Received Bad Status: '{0}'", line));
                return;
            }

            Group status = statusMatch.Groups["State"];

            if (status.Success)
            {
                Status = status.Value;
            }

            Vector3 NewMachinePosition, NewWorkPosition;
            bool update = false;

            Group mx = statusMatch.Groups["MX"], my = statusMatch.Groups["MY"], mz = statusMatch.Groups["MZ"];

            if (mx.Success)
            {
                NewMachinePosition = new Vector3(double.Parse(mx.Value, Constants.DecimalParseFormat), double.Parse(my.Value, Constants.DecimalParseFormat), double.Parse(mz.Value, Constants.DecimalParseFormat));

                if (MachinePosition != NewMachinePosition)
                    update = true;

                MachinePosition = NewMachinePosition;
            }

            Group wx = statusMatch.Groups["WX"], wy = statusMatch.Groups["WY"], wz = statusMatch.Groups["WZ"];
            Group wcox = statusMatch.Groups["WCOX"], wcoy = statusMatch.Groups["WCOY"], wcoz = statusMatch.Groups["WCOZ"];

            if (wx.Success)
            {
                NewWorkPosition = new Vector3(double.Parse(wx.Value, Constants.DecimalParseFormat), double.Parse(wy.Value, Constants.DecimalParseFormat), double.Parse(wz.Value, Constants.DecimalParseFormat));

                if (WorkPosition != NewWorkPosition)
                    update = true;

                WorkPosition = NewWorkPosition;
            }
            else if (wcox.Success)
            {
                NewWorkPosition = new Vector3(double.Parse(wcox.Value, Constants.DecimalParseFormat), double.Parse(wcoy.Value, Constants.DecimalParseFormat), double.Parse(wcoz.Value, Constants.DecimalParseFormat));

                if (WorkPosition != NewWorkPosition)
                    update = true;

                WorkPosition = NewWorkPosition;
            }

            if (update && Connected && PositionUpdateReceived != null)
                PositionUpdateReceived.Invoke();
        }

        //TODO: Removed compiled option
        private static Regex ProbeEx = new Regex(@"\[PRB:(?'MX'-?[0-9]+\.?[0-9]*),(?'MY'-?[0-9]+\.?[0-9]*),(?'MZ'-?[0-9]+\.?[0-9]*):(?'Success'0|1)\]");

        /// <summary>
        /// Parses a recevied probe report
        /// </summary>
        private void ParseProbe(string line)
        {
            if (ProbeFinished == null)
                return;

            Match probeMatch = ProbeEx.Match(line);
            Group mx = probeMatch.Groups["MX"];
            Group my = probeMatch.Groups["MY"];
            Group mz = probeMatch.Groups["MZ"];
            Group success = probeMatch.Groups["Success"];

            if (!probeMatch.Success || !(mx.Success & my.Success & mz.Success & success.Success))
            {
                NonFatalException.Invoke($"Received Bad Probe: '{line}'");
                return;
            }

            Vector3 ProbePos = new Vector3(double.Parse(mx.Value, Constants.DecimalParseFormat), double.Parse(my.Value, Constants.DecimalParseFormat), double.Parse(mz.Value, Constants.DecimalParseFormat));

            ProbePos += WorkPosition - MachinePosition;     //Mpos, Wpos only get updated by the same dispatcher, so this should be thread safe

            bool ProbeSuccess = success.Value == "1";

            ProbeFinished.Invoke(ProbePos, ProbeSuccess);
        }

        /// <summary>
        /// Reports error. This is there to offload the ExpandError function from the "Real-Time" worker thread to the application thread
        /// </summary>
        private void ReportError(string error)
        {
            if (NonFatalException != null)
                NonFatalException.Invoke(GrblErrorProvider.Instance.ExpandError(error));
        }

        private void RaiseEvent(Action<string> action, string param)
        {
            if (action == null)
                return;

            Services.DispatcherServices.Invoke(() =>
            {
                action(param);
            });
        }

        private void RaiseEvent(Action action)
        {
            if (action == null)
                return;

            Services.DispatcherServices.Invoke(action);
        }
    }
}
