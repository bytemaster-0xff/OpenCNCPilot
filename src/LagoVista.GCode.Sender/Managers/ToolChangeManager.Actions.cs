using LagoVista.Core.GCode.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class ToolChangeManager
    {
        private string _oldTool = "unknown";

        private async Task<bool> PerformToolChange()
        {
            if (!await Core.PlatformSupport.Services.Popups.ConfirmAsync("Tool Change", "Please confirm the probe is attached.\r\n\r\nThen press Yes to Continue or No to Abort."))
            {
                return false;
            }

            Machine.SendCommand("G0 Z10 F1000");
            Machine.ProbingManager.StartProbe();

            SpinWait.SpinUntil(() => Machine.Mode == OperatingMode.Manual, Machine.Settings.ProbeTimeoutSeconds * 1000);

            return Machine.ProbingManager.Status == ProbeStatus.Success;
        }

        public async Task HandleToolChange(ToolChangeCommand mcode)
        {
            Machine.SetMode(OperatingMode.Manual);
            Machine.SpindleOff();

            if (await Core.PlatformSupport.Services.Popups.ConfirmAsync("Tool Change", $"Start Tool Change cycle?\r\n\r\nLast Changed Tool: {_oldTool}\r\n\r\nNew tool: {mcode.ToolName} ({mcode.ToolSize})"))
            {
                Machine.SendCommand("G0 Z18 F1000");
                Machine.SendCommand("G0 X0 Y0 F1000");

                bool shouldRetry = true;
                while (shouldRetry)
                {
                    if (await PerformToolChange())
                    {
                        _oldTool = mcode.ToolSize;

                        await Core.PlatformSupport.Services.Popups.ShowAsync("IMPORTANT!\r\n\r\nConfirm Probe is Removed and Press Ok.");
                        Machine.SetMode(OperatingMode.SendingGCodeFile);
                        shouldRetry = false;
                    }
                    else
                    {
                        if (!await Core.PlatformSupport.Services.Popups.ConfirmAsync("Tool Change", "The Probing Cycle has Failed\r\n\r\nRetry Tool Change Cycle?"))
                        {
                            if (!await Core.PlatformSupport.Services.Popups.ConfirmAsync("Tool Change", "The Tool Change Process has Failed.\r\n\r\nPress Yes to Continue Job.\r\n\r\nNo to Abort"))
                            {
                                Machine.GCodeFileManager.ResetJob();
                                shouldRetry = false;
                            }
                            else
                            {
                                Machine.SetMode(OperatingMode.SendingGCodeFile);
                            }
                        }
                    }
                }
            }
            else
            {
                Machine.SetMode(OperatingMode.SendingGCodeFile);
            }
        }
    }
}
