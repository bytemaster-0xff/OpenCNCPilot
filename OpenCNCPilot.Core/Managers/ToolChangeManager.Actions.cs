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
            await Core.PlatformSupport.Services.Popups.ShowAsync("Confirm Probe Set and Press OK.");
            Machine.SendCommand("G0 Z10 F1000");
            Machine.ProbingManager.StartProbe();

            Debug.WriteLine("Spinning until timeout or back in manual mode " + DateTime.Now);

            SpinWait.SpinUntil(() => Machine.Mode == OperatingMode.Manual, Machine.Settings.ProbeTimeoutSeconds * 1000);

            Debug.WriteLine("Done Spinning or timed out" + DateTime.Now);

            return Machine.ProbingManager.Status == ProbeStatus.Success;
        }

        public async Task HandleToolChange(ToolChangeCommand mcode)
        {
            Machine.SetMode(OperatingMode.Manual);
            Machine.SpindleOff();

            if (await Core.PlatformSupport.Services.Popups.ConfirmAsync("Tool Change", "Start Tool Change cycle?  Last Changed Tool: " + _oldTool + "\nNew tool: " + mcode.ToolSize))
            {
                Machine.SendCommand("G0 Z30 F1000");
                Machine.SendCommand("G0 X0 Y0 F1000");

                bool shouldRetry = true;
                while (shouldRetry)
                {
                    if (await PerformToolChange())
                    {
                        _oldTool = mcode.ToolSize;

                        Debug.WriteLine("Tool Change Proble Success, Continue");
                        await Core.PlatformSupport.Services.Popups.ShowAsync("WARNING - Confirm Probe is Removed and Press OK.");
                        Machine.SetMode(OperatingMode.SendingGCodeFile);
                        shouldRetry = false;
                    }
                    else
                    {
                        if (!await Core.PlatformSupport.Services.Popups.ConfirmAsync("Probe Failed", "Try Again?"))
                        {
                            if (!await Core.PlatformSupport.Services.Popups.ConfirmAsync("Probe Failed", "Press OK to Continue Job, Cancel to Abort"))
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
