using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class ProbingManager
    {
        ITimer _timer;

        private static Regex ProbeEx = new Regex(@"\[PRB:(?'MX'-?[0-9]+\.?[0-9]*),(?'MY'-?[0-9]+\.?[0-9]*),(?'MZ'-?[0-9]+\.?[0-9]*):(?'Success'0|1)\]");

        /// <summary>
        /// Parses a recevied probe report
        /// </summary>
        public Vector3? ParseProbeLine(string line)
        {
            Match probeMatch = ProbeEx.Match(line);
            Group mx = probeMatch.Groups["MX"];
            Group my = probeMatch.Groups["MY"];
            Group mz = probeMatch.Groups["MZ"];
            Group success = probeMatch.Groups["Success"];

            if (!probeMatch.Success || !(mx.Success & my.Success & mz.Success & success.Success))
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, $"Received Bad Probe: '{line}'");
                return null;
            }

            var probePos = new Vector3(double.Parse(mx.Value, Constants.DecimalParseFormat), double.Parse(my.Value, Constants.DecimalParseFormat), double.Parse(mz.Value, Constants.DecimalParseFormat));

            probePos += Machine.WorkPosition - Machine.MachinePosition;     //Mpos, Wpos only get updated by the same dispatcher, so this should be thread safe
            return probePos;
        }

        public void StartProbe()
        {
            if (Machine.SetMode(OperatingMode.ProbingHeight))
            {
                _timer = Core.PlatformSupport.Services.TimerFactory.Create(TimeSpan.FromSeconds(Machine.Settings.ProbeTimeoutSeconds));
                _timer.Interval = TimeSpan.FromDays(Machine.Settings.ProbeTimeoutSeconds);
                _timer.Start();
                _timer.Tick += _timer_Tick;
                Machine.SendCommand($"G38.3Z-{Machine.Settings.ProbeMaxDepth.ToString("0.###", Constants.DecimalOutputFormat)}F{Machine.Settings.ProbeFeed.ToString("0.#", Constants.DecimalOutputFormat)}");
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            Machine.SetMode(OperatingMode.Manual);
            Machine.AddStatusMessage(StatusMessageTypes.Warning, $"Probing timed out after {Machine.Settings.ProbeTimeoutSeconds} sec.");

            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }

        public void CancelProbe()
        {
            Machine.AddStatusMessage(StatusMessageTypes.Info, $"Probing Manually Cancelled");

            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }

            Machine.SetMode(OperatingMode.Manual);
        }

        public void SetZAxis(double z)
        {
            Machine.SendCommand($"G92 Z{Machine.Settings.ProbeOffset.ToString("0.###", Constants.DecimalOutputFormat)}");
       }

        public void ProbeCompleted(Vector3 position)
        {
            Machine.AddStatusMessage(StatusMessageTypes.Info, $"Probing Completed Offset {position.Z}");

            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
                SetZAxis(position.Z);
            Machine.SetMode(OperatingMode.Manual);
        }

        public void ProbeFailed()
        {
            Machine.AddStatusMessage(StatusMessageTypes.Info, $"Probing Failed, Invalid Response");

            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }
    }
}
