using LagoVista.Core.Models.Drawing;
using System;
using System.Text.RegularExpressions;

namespace LagoVista.GCode.Sender
{
    //TODO: Removed compiled option

    public partial class Machine
    {
        //private static Regex StatusEx = new Regex(@"<(?'State'Idle|Run|Hold|Home|Alarm|Check|Door)(:[0-9])?(?:.MPos:(?'MX'-?[0-9\.]*),(?'MY'-?[0-9\.]*),(?'MZ'-?[0-9\.]*))?(?:,WPos:(?'WX'-?[0-9\.]*),(?'WY'-?[0-9\.]*),(?'WZ'-?[0-9\.]*))?(?:,Buf:(?'Buf'[0-9]*))?(?:,RX:(?'RX'[0-9]*))?(?:,Ln:(?'L'[0-9]*))?(?:,F:(?'F'[0-9\.]*))?(?:,Lim:(?'Lim'[0-1]*))?(?:,Ctl:(?'Ctl'[0-1]*))?(?:.FS:(?'FSX'-?[0-9\.]*),(?'FSY'-?[0-9\.]*))?(?:.Pn.P)?(?:.WCO:(?'WCOX'-?[0-9\.]*),(?'WCOY'-?[0-9\.]*),(?'WCOZ'-?[0-9\.]*))?(?:.Ov:(?'OVX'-?[0-9\.]*),(?'OVY'-?[0-9\.]*),(?'OVZ'-?[0-9\.]*))?>");
        private static Regex StatusEx = new Regex(@"<(?'State'Idle|Run|Hold|Home|Alarm|Check|Door)(:[0-9])?(?:.MPos:(?'MX'-?[0-9\.]*),(?'MY'-?[0-9\.]*),(?'MZ'-?[0-9\.]*))?(?:,WPos:(?'WX'-?[0-9\.]*),(?'WY'-?[0-9\.]*),(?'WZ'-?[0-9\.]*))?(?:,Buf:(?'Buf'[0-9]*))?(?:,RX:(?'RX'[0-9]*))?(?:,Ln:(?'L'[0-9]*))?(?:,F:(?'F'[0-9\.]*))?(?:,Lim:(?'Lim'[0-1]*))?(?:,Ctl:(?'Ctl'[0-1]*))?(?:.FS:(?'FSX'-?[0-9\.]*),(?'FSY'-?[0-9\.]*))?(?:.Pn.P)?(?:.Pn:.)?(?:.WCO:(?'WCOX'-?[0-9\.]*),(?'WCOY'-?[0-9\.]*),(?'WCOZ'-?[0-9\.]*))?(?:.Ov:(?'OVX'-?[0-9\.]*),(?'OVY'-?[0-9\.]*),(?'OVZ'-?[0-9\.]*))?>");

        private static Regex CurrentPositionRegEx = new Regex(@"X:(?'MX'-?[0-9\.]*)\s?Y:(?'MY'-?[0-9\.]*)\s?Z:(?'MZ'-?[0-9\.]*)\s?E:(?'E'-?[0-9\.]*)\s?Count\s?X:(?'WX'.-?[0-9\.]*)\s?Y:(?'WY'.-?[0-9\.]*)\s?Z:(?'WZ'.-?[0-9\.]*)");

        private static Regex LagoVistaStatusRegEx1 = new Regex(@"<(?'State'Idle|Alarm|Run|Hold|Home|Check|Door)(:[0-9])?(?:.MPos:(?'MX'-?[0-9\.]*),(?'MY'-?[0-9\.]*),(?'MT0'-?[0-9\.]*),(?'MT1'-?[0-9\.]*),(?'MT2'-?[0-9\.]*))?(?:,T:(?'T'-?[0-9])),?(?:,P:(?'P'-?[0-9]))>");
        private static Regex LagoVistaStatusRegEx2 = new Regex(@"<?(?:WPos:(?'WX'-?[0-9\.]*),(?'WY'-?[0-9\.]*),(?'WT0'-?[0-9\.]*),(?'WT1'-?[0-9\.]*),(?'WT2'-?[0-9\.]*))>");

        private static Regex LagoVistaErrorRegEx = new Regex(@"<(?'State'Alarm|Message|EndStop)?:(?'Msg'[\w]*)>");

        private static Regex LagoVistaEndStopMsgRegEx = new Regex(@"<EndStop:(?'Axis'[\w]*),(?'Stop'[\w]*)>");

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
                {
                    MachinePosition = newMachinePosition;
                }
            }

            Group wx = grblStatusMatch.Groups["WX"], wy = grblStatusMatch.Groups["WY"], wz = grblStatusMatch.Groups["WZ"];
            Group wcox = grblStatusMatch.Groups["WCOX"], wcoy = grblStatusMatch.Groups["WCOY"], wcoz = grblStatusMatch.Groups["WCOZ"];

            if (wx.Success)
            {
                var newWorkPosition = new Vector3(double.Parse(wx.Value, Constants.DecimalParseFormat), double.Parse(wy.Value, Constants.DecimalParseFormat), double.Parse(wz.Value, Constants.DecimalParseFormat));

                if (WorkPositionOffset != newWorkPosition)
                {
                    WorkPositionOffset = newWorkPosition;
                }
            }
            else if (wcox.Success)
            {
                var newWorkPosition = new Vector3(double.Parse(wcox.Value, Constants.DecimalParseFormat), double.Parse(wcoy.Value, Constants.DecimalParseFormat), double.Parse(wcoz.Value, Constants.DecimalParseFormat));

                if (WorkPositionOffset != newWorkPosition)
                {
                    WorkPositionOffset = newWorkPosition;
                }
            }

            return true;
        }

        int currentTool;

        public bool ParseLagoVistaLine(String line)
        {
            var lgvStatusMatch1 = LagoVistaStatusRegEx1.Match(line);
            var lgvStatusMatch2 = LagoVistaStatusRegEx2.Match(line);
            var lgvErrorMatch = LagoVistaErrorRegEx.Match(line);
            var endStopMessage = LagoVistaEndStopMsgRegEx.Match(line);


            if (lgvStatusMatch1.Success)
            {
                Group status = lgvStatusMatch1.Groups["State"];

                if (status.Success)
                {
                    Status = status.Value;
                }

                Group mx = lgvStatusMatch1.Groups["MX"],
                    my = lgvStatusMatch1.Groups["MY"],
                    mt0 = lgvStatusMatch1.Groups["MT0"],
                    mt1 = lgvStatusMatch1.Groups["MT1"],
                    mt2 = lgvStatusMatch1.Groups["MT2"];
             

                Group t = lgvStatusMatch1.Groups["T"],
                    p = lgvStatusMatch1.Groups["P"];


                currentTool = int.Parse(t.Value);

                var newMachinePosition = new Vector3(double.Parse(mx.Value, Constants.DecimalParseFormat), double.Parse(my.Value, Constants.DecimalParseFormat), 0);

                if (MachinePosition != newMachinePosition)
                {
                    MachinePosition = newMachinePosition;
                }

                Tool0 = double.Parse(mt0.Value, Constants.DecimalParseFormat);
                Tool1 = double.Parse(mt1.Value, Constants.DecimalParseFormat);
                Tool2 = double.Parse(mt2.Value, Constants.DecimalParseFormat);           

                return true;
            }
            else if (lgvStatusMatch2.Success)
            {
                Group wx = lgvStatusMatch2.Groups["WX"],
                 wy = lgvStatusMatch2.Groups["WY"],
                 wt0 = lgvStatusMatch2.Groups["WT0"],
                 wt1 = lgvStatusMatch2.Groups["WT1"],
                 wt2 = lgvStatusMatch2.Groups["WT2"];

                Tool0Offset = double.Parse(wt0.Value, Constants.DecimalParseFormat);
                Tool1Offset = double.Parse(wt1.Value, Constants.DecimalParseFormat);
                Tool2Offset = double.Parse(wt2.Value, Constants.DecimalParseFormat);

                var newWorkPosition = new Vector3(double.Parse(wx.Value, Constants.DecimalParseFormat), double.Parse(wy.Value, Constants.DecimalParseFormat), double.Parse(currentTool == 0 ? wt0.Value : wt1.Value, Constants.DecimalParseFormat));

                if (WorkPositionOffset != newWorkPosition)
                {
                    WorkPositionOffset = newWorkPosition;
                }
                return true;
            }
            else if (lgvErrorMatch.Success)
            {
                Group state = lgvStatusMatch1.Groups["State"], msg = lgvStatusMatch1.Groups["Msg"];
                if (state.Success)
                {
                    Status = state.Value;
                    Mode = OperatingMode.Alarm;
                    AddStatusMessage(StatusMessageTypes.Warning, "Returned: " + Status, MessageVerbosityLevels.Normal);
                    return true;
                }

            }
            else if (endStopMessage.Success)
            {
                Group axis = endStopMessage.Groups["Axis"], stop = endStopMessage.Groups["Stop"];
                if (axis.Success)
                {
                    AddStatusMessage(StatusMessageTypes.FatalError, "Endstop Hit: " + axis.Value + " " + stop.Value);
                    Mode = OperatingMode.Alarm;
                    return true;
                }
            }

            return false;

        }

        public bool ParseLine(String line)
        {
            var m114PositionMatch = CurrentPositionRegEx.Match(line);
            if (!m114PositionMatch.Success)
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

                if (WorkPositionOffset != newWorkPosition)
                {
                    //WorkPositionOffset = newWorkPosition;
                }
            }

            return true;
        }


    }
}
