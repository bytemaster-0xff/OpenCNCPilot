using LagoVista.Core.Models.Drawing;
using System;
using System.Text.RegularExpressions;

namespace LagoVista.GCode.Sender
{
    //TODO: Removed compiled option

    public partial class Machine
    {
        private static Regex StatusEx = new Regex(@"<(?'State'Idle|Run|Hold|Home|Alarm|Check|Door)(?:.MPos:(?'MX'-?[0-9\.]*),(?'MY'-?[0-9\.]*),(?'MZ'-?[0-9\.]*))?(?:,WPos:(?'WX'-?[0-9\.]*),(?'WY'-?[0-9\.]*),(?'WZ'-?[0-9\.]*))?(?:,Buf:(?'Buf'[0-9]*))?(?:,RX:(?'RX'[0-9]*))?(?:,Ln:(?'L'[0-9]*))?(?:,F:(?'F'[0-9\.]*))?(?:,Lim:(?'Lim'[0-1]*))?(?:,Ctl:(?'Ctl'[0-1]*))?(?:.FS:(?'FSX'-?[0-9\.]*),(?'FSY'-?[0-9\.]*))?(?:.WCO:(?'WCOX'-?[0-9\.]*),(?'WCOY'-?[0-9\.]*),(?'WCOZ'-?[0-9\.]*))?(?:.Ov:(?'OVX'-?[0-9\.]*),(?'OVY'-?[0-9\.]*),(?'OVZ'-?[0-9\.]*))?>");
        private static Regex CurrentPositionRegEx = new Regex(@"X:(?'MX'-?[0-9\.]*)Y:(?'MY'-?[0-9\.]*)Z:(?'MZ'-?[0-9\.]*)E:(?'E'-?[0-9\.]*) Count X:(?'WX'.-?[0-9\.]*)Y:(?'WY'.-?[0-9\.]*)Z:(?'WZ'.-?[0-9\.]*)");

        /// <summary>
        /// Parses a recevied status report (answer to '?')
        /// </summary>
        private bool ParseStatus(string line)
        {
            Match grblStatusMatch = StatusEx.Match(line);

            if (!grblStatusMatch.Success)
            {
                return false;
            }

            Group status = grblStatusMatch.Groups["State"];

            if (status.Success)
            {
                Status = status.Value;
            }

            Group mx = grblStatusMatch.Groups["MX"], my = grblStatusMatch.Groups["MY"], mz = grblStatusMatch.Groups["MZ"];

            if (mx.Success)
            {
                var newMachinePosition = new Vector3(double.Parse(mx.Value, Constants.DecimalParseFormat), double.Parse(my.Value, Constants.DecimalParseFormat), double.Parse(mz.Value, Constants.DecimalParseFormat));

                if (MachinePosition != newMachinePosition)

                MachinePosition = newMachinePosition;
            }

            Group wx = grblStatusMatch.Groups["WX"], wy = grblStatusMatch.Groups["WY"], wz = grblStatusMatch.Groups["WZ"];
            Group wcox = grblStatusMatch.Groups["WCOX"], wcoy = grblStatusMatch.Groups["WCOY"], wcoz = grblStatusMatch.Groups["WCOZ"];

            if (wx.Success)
            {
                var newWorkPosition = new Vector3(double.Parse(wx.Value, Constants.DecimalParseFormat), double.Parse(wy.Value, Constants.DecimalParseFormat), double.Parse(wz.Value, Constants.DecimalParseFormat));

                if (WorkPosition != newWorkPosition)
                {

                    WorkPosition = newWorkPosition;
                }
            }
            else if (wcox.Success)
            {
                var newWorkPosition = new Vector3(double.Parse(wcox.Value, Constants.DecimalParseFormat), double.Parse(wcoy.Value, Constants.DecimalParseFormat), double.Parse(wcoz.Value, Constants.DecimalParseFormat));

                if (WorkPosition != newWorkPosition)
                {
                    WorkPosition = newWorkPosition;
                }
            }

            return true;
        }

        public bool ParseLine(String line)
        {
            var m114PositionMatch = CurrentPositionRegEx.Match(line);
            if(!m114PositionMatch.Success)
            {
                return false;
            }

            Group mx = m114PositionMatch.Groups["MX"], my = m114PositionMatch.Groups["MY"], mz = m114PositionMatch.Groups["MZ"];
            Group wx = m114PositionMatch.Groups["WX"], wy = m114PositionMatch.Groups["WY"], wz = m114PositionMatch.Groups["WZ"];

            if (mx.Success)
            {
                var newMachinePosition = new Vector3(double.Parse(mx.Value, Constants.DecimalParseFormat), double.Parse(my.Value, Constants.DecimalParseFormat), double.Parse(mz.Value, Constants.DecimalParseFormat));

                if (MachinePosition != newMachinePosition)
                {
                    MachinePosition = newMachinePosition;
                }
            }

            if (wx.Success)
            {
                var newWorkPosition = new Vector3(double.Parse(wx.Value, Constants.DecimalParseFormat), double.Parse(wy.Value, Constants.DecimalParseFormat), double.Parse(wz.Value, Constants.DecimalParseFormat));

                if (WorkPosition != newWorkPosition)
                {
                    WorkPosition = newWorkPosition;
                }
            }

            return true;
        }

      
    }
}
