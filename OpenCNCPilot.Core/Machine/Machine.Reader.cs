using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender
{
    public partial class Machine
    {
        private void ProcessResponseLine(String line)
        {
            if (String.IsNullOrEmpty(line))
            {
                return;
            }

            Debug.WriteLine(DateTime.Now.ToString() + "  >>> " + line);

            if (line == "ok")
            {
                if (JobManager.HasValidFile)
                {
                    lock (this)
                    {
                        JobManager.CommandAcknowledged();

                        lock (_queueAccessLocker)
                        {
                            if (_sentQueue.Any())
                            {
                                var sentLine = _sentQueue.Dequeue();
                                UnacknowledgedBytesSent -= (sentLine.Length + 1);
                            }
                        }

                        if (JobManager.IsCompleted)
                        {
                            Mode = OperatingMode.Manual;
                            JobManager = null;
                        }
                    }
                }
                else
                {
                    lock (_queueAccessLocker)
                    {
                        if (_sentQueue.Any())
                        {
                            var sentLine = _sentQueue.Dequeue();
                            UnacknowledgedBytesSent -= (sentLine.Length + 1);
                            return;
                        }
                    }

                    LagoVista.Core.PlatformSupport.Services.Logger.Log(LagoVista.Core.PlatformSupport.LogLevel.Warning, "Machine_Work", "Received OK without anything in the Sent Buffer");
                    AddStatusMessage(StatusMessageTypes.Warning, "Unexpected OK");
                    UnacknowledgedBytesSent = 0;
                }
            }
            else if (line.Contains("endstops"))
            {
                AddStatusMessage(StatusMessageTypes.FatalError, line);
            }
            else if (line != null)
            {
                if (line.StartsWith("error: "))
                {
                    if (_sentQueue.Count != 0)
                    {
                        var errorline = _sentQueue.Dequeue();

                        RaiseEvent(ReportError, $"{line}: {errorline}");
                        UnacknowledgedBytesSent -= errorline.Length + 1;
                    }
                    else
                    {
                        if ((DateTime.Now - _connectTime).TotalMilliseconds > 200)
                            RaiseEvent(ReportError, $"Received <{line}> without anything in the Sent Buffer");

                        UnacknowledgedBytesSent = 0;
                    }

                    Mode = OperatingMode.Manual;
                }
                else if (line.StartsWith("<"))
                    RaiseEvent(ParseStatus, line);
                else if (line.StartsWith("[PRB:"))
                {
                    switch(Mode)
                    {
                        case OperatingMode.ProbingHeight:
                            break;
                        case OperatingMode.ProbingHeightMap:
                            HeightMapProbingManager.ProbeCompleted(line);
                            break;
                        default:
                            AddStatusMessage(StatusMessageTypes.Warning, "Unexpected PRM return message.");
                            break;
                    }
                    RaiseEvent(ParseProbe, line);
                }
                else if (line.StartsWith("["))
                {
                    UpdateStatus(line);

                    AddStatusMessage(StatusMessageTypes.ReceviedLine, line);
                }
                else if (line.StartsWith("ALARM"))
                {
                    AddStatusMessage(StatusMessageTypes.FatalError, line);
                    Mode = OperatingMode.Manual;
                }
                else if (line.Length > 0)
                {
                    if (!ParseLine(line))
                    {
                        AddStatusMessage(StatusMessageTypes.ReceviedLine, line);
                    }
                    lock (_queueAccessLocker)
                    {
                        if (_sentQueue.Any())
                        {
                            var sentLine = _sentQueue.Dequeue();
                            UnacknowledgedBytesSent -= (sentLine.Length + 1);
                        }
                    }
                }
            }
            else
            {
                RaiseEvent(ReportError, $"Empty Response From Machine.");
            }
        }
    }
}
