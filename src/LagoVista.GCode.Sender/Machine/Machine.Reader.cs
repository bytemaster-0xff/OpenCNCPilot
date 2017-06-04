﻿using LagoVista.GCode.Sender.Util;
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
        private StringBuilder _messageBuffer = new StringBuilder();
     
        private void ParseMessage(string fullMessageLine)
        {
            Debug.WriteLine(fullMessageLine);

            if (fullMessageLine.StartsWith("ok"))
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
            else if (fullMessageLine.Contains("endstops"))
            {
                AddStatusMessage(StatusMessageTypes.FatalError, fullMessageLine);
            }
            else if (fullMessageLine != null)
            {
                if (fullMessageLine.StartsWith("error:"))
                {
                    var errorline = _sentQueue.Any() ? _sentQueue.Dequeue() : "?????";


                    var errNumbRegEx = new Regex("error:(?'ErrorCode'-?[0-9\\.]*)?");

                    var errMatch = errNumbRegEx.Match(fullMessageLine);
                    if (errMatch.Success)
                    {
                        var strErrorCode = errMatch.Groups["ErrorCode"].Value;
                        var err = GrblErrorProvider.Instance.GetErrorMessage(Convert.ToInt32(strErrorCode));
                        AddStatusMessage(StatusMessageTypes.Warning, err, MessageVerbosityLevels.Normal);
                    }
                    else
                        AddStatusMessage(StatusMessageTypes.Warning, $"{fullMessageLine}: {errorline}", MessageVerbosityLevels.Normal);

                    if (_sentQueue.Count != 0)
                    {
                        var sentLine = _sentQueue.Dequeue();
                        UnacknowledgedBytesSent -= sentLine.Length + 1;
                    }
                    else
                    {
                        if ((DateTime.Now - _connectTime).TotalMilliseconds > 200)
                        {
                            AddStatusMessage(StatusMessageTypes.Warning, $"Received <{fullMessageLine}> without anything in the Sent Buffer", MessageVerbosityLevels.Normal);
                        }

                        UnacknowledgedBytesSent = 0;
                    }

                    Mode = OperatingMode.Manual;
                }
                else if (fullMessageLine.StartsWith("<"))
                {
                    if(Settings.MachineType == FirmwareTypes.LagoVista || Settings.MachineType == FirmwareTypes.LagoVista_PnP)
                    {
                        if (ParseLagoVistaLine(fullMessageLine))
                        {
                            AddStatusMessage(StatusMessageTypes.ReceivedLine, fullMessageLine, MessageVerbosityLevels.Diagnostics);
                        }
                    }
                    else if (ParseStatus(fullMessageLine))
                    {
                        AddStatusMessage(StatusMessageTypes.ReceivedLine, fullMessageLine, MessageVerbosityLevels.Diagnostics);
                    }
                    else if (ParseLine(fullMessageLine))
                    {
                        AddStatusMessage(StatusMessageTypes.ReceivedLine, fullMessageLine, MessageVerbosityLevels.Diagnostics);
                    }
                    else
                    {
                        AddStatusMessage(StatusMessageTypes.ReceivedLine, fullMessageLine);
                    }
                }
                else if (fullMessageLine.StartsWith("[PRB:"))
                {
                    var probeResult = ProbingManager.ParseProbeLine(fullMessageLine);

                    switch (Mode)
                    {
                        case OperatingMode.ProbingHeight:
                            ProbingManager.ProbeCompleted(probeResult.Value);
                            break;
                        case OperatingMode.ProbingHeightMap:
                            HeightMapManager.ProbeCompleted(probeResult.Value);
                            break;
                        default:
                            AddStatusMessage(StatusMessageTypes.Warning, "Unexpected PRB return message.");
                            break;
                    }
                }
                else if (fullMessageLine.StartsWith("["))
                {
                    UpdateStatus(fullMessageLine);

                    AddStatusMessage(StatusMessageTypes.ReceivedLine, fullMessageLine);
                }
                else if (fullMessageLine.StartsWith("ALARM"))
                {
                    AddStatusMessage(StatusMessageTypes.FatalError, fullMessageLine);
                    Mode = OperatingMode.Manual;
                }
                else if (fullMessageLine.Length > 0)
                {
                    if (!ParseLine(fullMessageLine))
                    {
                        AddStatusMessage(StatusMessageTypes.ReceivedLine, fullMessageLine);
                    }

                    /*
                     * Not sure why we would dequeue if we couldn't parse the line...may need to revisit *
                    lock (_queueAccessLocker)
                    {
                        if (_sentQueue.Any())
                        {
                            var sentLine = _sentQueue.Dequeue();
                            UnacknowledgedBytesSent -= (sentLine.Length + 1);
                        }
                    }*/
                }
            }
            else
            {
                AddStatusMessage(StatusMessageTypes.Warning, $"Empty Response From Machine.", MessageVerbosityLevels.Normal);
            }
        }


        private void ProcessResponseLine(String line)
        {
            
            if (String.IsNullOrEmpty(line))
            {
                return;
            }

            ParseMessage(line);

/*            foreach (var ch in line.ToCharArray())
            {
                if (ch == '\r')
                {
                    ParseMessage(_messageBuffer.ToString());
                    _messageBuffer.Clear();
                }
                else
                {
                    _messageBuffer.Append(ch);
                }
            }*/
        }
    }
}
