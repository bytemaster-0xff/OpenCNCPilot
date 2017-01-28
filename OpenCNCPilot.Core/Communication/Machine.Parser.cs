using OpenCNCPilot.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenCNCPilot.Core.Communication
{
    //TODO: Removed compiled option

    public partial class Machine
    {
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

    }
}
