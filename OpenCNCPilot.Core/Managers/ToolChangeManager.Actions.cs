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
    public  partial class ToolChangeManager
    {
        public async Task HandleToolChange(ToolChangeCommand mcode)
        {
            Debug.WriteLine("Starting Tool Change");

            Machine.SpindleOff();
            Machine.SendCommand("G0 Z30 F1000");
            Machine.SendCommand("G0 X0 Y0 F1000");
            await Core.PlatformSupport.Services.Popups.ShowAsync("Tool Change Required\nChange Tool to: " + mcode.ToolSize + " and set probe");
            Machine.SendCommand("G0 Z10 F1000");
            Machine.SetMode(OperatingMode.Manual);
            Machine.ProbingManager.StartProbe();

            Debug.WriteLine("Spinning until timeout or back in manual mode " + DateTime.Now);

            SpinWait.SpinUntil(() => Machine.Mode == OperatingMode.Manual, Machine.Settings.ProbeTimeoutSeconds * 1000);

            Debug.WriteLine("Done Spinning or timed out" + DateTime.Now);

            if (Machine.ProbingManager.Status == ProbeStatus.Success)
            {
                Debug.WriteLine("Probe success, continus");

                await Core.PlatformSupport.Services.Popups.ShowAsync("WARNING - REMOVE PROBING and press OK");
                await Core.PlatformSupport.Services.Popups.ShowAsync("WARNING - Confirm Probe is Removed");
                Machine.SetMode(OperatingMode.SendingGCodeFile);
            }
            else
            {
                Debug.WriteLine("Probe failed, abort");

                await Core.PlatformSupport.Services.Popups.ShowAsync("Probing Failed, please check machine.");
                Machine.GCodeFileManager.ResetJob();
            }
        }
    }
}
