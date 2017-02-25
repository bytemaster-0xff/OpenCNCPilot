using LagoVista.GCode.Sender.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

            if (line.StartsWith("ok"))
            {
                if (GCodeFileManager.HasValidFile && Mode == OperatingMode.SendingGCodeFile)
                {
                    lock (this)
                    {
                        GCodeFileManager.CommandAcknowledged();

                        lock (_queueAccessLocker)
                        {
                            if (_sentQueue.Any())
                            {
                                var sentLine = _sentQueue.Dequeue();
                                Debug.WriteLine("<<<<< " + sentLine);
                                UnacknowledgedBytesSent -= (sentLine.Length + 1);
                            }
                        }

                        if (GCodeFileManager.IsCompleted)
                        {
                            Mode = OperatingMode.Manual;
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
                    UnacknowledgedBytesSent = 0;
                }
            }
            else if (line.Contains("endstops"))
            {
                AddStatusMessage(StatusMessageTypes.FatalError, line);
            }
            else if (line != null)
            {
                if (line.StartsWith("error:"))
                {
                    var errorline = _sentQueue.Dequeue();

                    var errNumbRegEx = new Regex("error:(?'ErrorCode'-?[0-9\\.]*)?");

                    var errMatch = errNumbRegEx.Match(line);
                    if (errMatch.Success)
                    {
                        var strErrorCode = errMatch.Groups["ErrorCode"].Value;
                        var err = GrblErrorProvider.Instance.GetErrorMessage(Convert.ToInt32(strErrorCode));
                        AddStatusMessage(StatusMessageTypes.Warning, err, MessageVerbosityLevels.Normal);
                    }
                    else
                        AddStatusMessage(StatusMessageTypes.Warning, $"{line}: {errorline}", MessageVerbosityLevels.Normal);

                    if (_sentQueue.Count != 0)
                    {
                        var sentLine = _sentQueue.Dequeue();
                        UnacknowledgedBytesSent -= sentLine.Length + 1;
                    }
                    else
                    {
                        if ((DateTime.Now - _connectTime).TotalMilliseconds > 200)
                        {
                            AddStatusMessage(StatusMessageTypes.Warning, $"Received <{line}> without anything in the Sent Buffer", MessageVerbosityLevels.Normal);
                        }

                        UnacknowledgedBytesSent = 0;
                    }

                    Mode = OperatingMode.Manual;
                }
                else if (line.StartsWith("<"))
                {
                    if (ParseStatus(line))
                    {
                        AddStatusMessage(StatusMessageTypes.ReceviedLine, line, MessageVerbosityLevels.Diagnostics);
                    }
                    else if (ParseLine(line))
                    {
                        AddStatusMessage(StatusMessageTypes.ReceviedLine, line, MessageVerbosityLevels.Diagnostics);
                    }
                    else
                    {
                        AddStatusMessage(StatusMessageTypes.ReceviedLine, line);
                    }
                }
                else if (line.StartsWith("[PRB:"))
                {
                    var probeResult = ProbingManager.ParseProbeLine(line);

                    switch (Mode)
                    {
                        case OperatingMode.ProbingHeight:
                            ProbingManager.ProbeCompleted(probeResult.Value);
                            break;
                        case OperatingMode.ProbingHeightMap:
                            HeightMapManager.ProbeCompleted(probeResult.Value);
                            break;
                        default:
                            AddStatusMessage(StatusMessageTypes.Warning, "Unexpected PRM return message.");
                            break;
                    }
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
                AddStatusMessage(StatusMessageTypes.Warning, $"Empty Response From Machine.", MessageVerbosityLevels.Normal);
            }
        }
    }
}
