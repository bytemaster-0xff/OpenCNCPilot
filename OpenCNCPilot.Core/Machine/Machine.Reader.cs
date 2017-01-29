using System;
using System.Collections.Generic;
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
            else if (line != null)
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
                        if ((DateTime.Now - _connectTime).TotalMilliseconds > 200)
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
                {
                    if (!ParseLine(line))
                    {
                        RaiseEvent(LineReceived, line);
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
