using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace LagoVista.GCode.Sender
{
    //TODO: Removed compiled option

    public partial class Machine
    {
        //private static Regex StatusEx = new Regex(@"<(?'State'Idle|Run|Hold|Home|Alarm|Check|Door)(:[0-9])?(?:.MPos:(?'MX'-?[0-9\.]*),(?'MY'-?[0-9\.]*),(?'MZ'-?[0-9\.]*))?(?:,WPos:(?'WX'-?[0-9\.]*),(?'WY'-?[0-9\.]*),(?'WZ'-?[0-9\.]*))?(?:,Buf:(?'Buf'[0-9]*))?(?:,RX:(?'RX'[0-9]*))?(?:,Ln:(?'L'[0-9]*))?(?:,F:(?'F'[0-9\.]*))?(?:,Lim:(?'Lim'[0-1]*))?(?:,Ctl:(?'Ctl'[0-1]*))?(?:.FS:(?'FSX'-?[0-9\.]*),(?'FSY'-?[0-9\.]*))?(?:.Pn.P)?(?:.WCO:(?'WCOX'-?[0-9\.]*),(?'WCOY'-?[0-9\.]*),(?'WCOZ'-?[0-9\.]*))?(?:.Ov:(?'OVX'-?[0-9\.]*),(?'OVY'-?[0-9\.]*),(?'OVZ'-?[0-9\.]*))?>");
        private static Regex StatusEx = new Regex(@"<(?'State'Idle|Run|Hold|Home|Alarm|Check|Door)(:[0-9])?(?:.MPos:(?'MX'-?[0-9\.]*),(?'MY'-?[0-9\.]*),(?'MZ'-?[0-9\.]*))?(?:,WPos:(?'WX'-?[0-9\.]*),(?'WY'-?[0-9\.]*),(?'WZ'-?[0-9\.]*))?(?:,Buf:(?'Buf'[0-9]*))?(?:,RX:(?'RX'[0-9]*))?(?:,Ln:(?'L'[0-9]*))?(?:,F:(?'F'[0-9\.]*))?(?:,Lim:(?'Lim'[0-1]*))?(?:,Ctl:(?'Ctl'[0-1]*))?(?:.FS:(?'FSX'-?[0-9\.]*),(?'FSY'-?[0-9\.]*))?(?:.Pn.P)?(?:.Pn:.)?(?:.WCO:(?'WCOX'-?[0-9\.]*),(?'WCOY'-?[0-9\.]*),(?'WCOZ'-?[0-9\.]*))?(?:.Ov:(?'OVX'-?[0-9\.]*),(?'OVY'-?[0-9\.]*),(?'OVZ'-?[0-9\.]*))?>");

        private static Regex CurrentPositionRegEx = new Regex(@"X:(?'MX'-?[0-9\.]*)\s?Y:(?'MY'-?[0-9\.]*)\s?Z:(?'MZ'-?[0-9\.]*)\s?E:(?'E'-?[0-9\.]*)\s?Count\s?X:(?'WX'.-?[0-9\.]*)\s?Y:(?'WY'.-?[0-9\.]*)\s?Z:(?'WZ'.-?[0-9\.]*)");

        private static Regex LagoVistaStatusRegEx1 = new Regex(@"<(?'State'Idle|Alarm|Run|Hold|Home|Check|Door)(:[0-9])?(?:.m:(?'MX'-?[0-9\.]*),(?'MY'-?[0-9\.]*),(?'MT0'-?[0-9\.]*),(?'MT1'-?[0-9\.]*),(?'MT2'-?[0-9\.]*)),(?'VT'camera|tool1|tool2)>");
        private static Regex LagoVistaStatusRegEx2 = new Regex(@"<(?:w:(?'WX'-?[0-9\.]*),(?'WY'-?[0-9\.]*),(?'WT0'-?[0-9\.]*),(?'WT1'-?[0-9\.]*),(?'WT2'-?[0-9\.]*))>");
        private static Regex LagoVistaStatusRegEx3 = new Regex(@"<(?:TL:(?'TL'-?[01]*)),(?:BL:(?'BL'-?[01]*)),(?:VA:(?'VA'-?[01]*)),(?:SU:(?'SU'-?[01]*)),(?:EX:(?'EX'-?[01]*)),(?:TO:(?'TO'-?[0-9]*)),(?:PA:(?'PA'-?[01]*))>");

        private static Regex LagoVistaMovementMode = new Regex(@"<mode:(?'mode'absolute|relative)>");

        private static Regex LagoVistaEndStop = new Regex(@"<endstops:(?'XMIN'[01]?),(?'XMAX'[01]?),(?'YMIN'[01]?),(?'YMAX'[01]?),(?'ZMIN1'[01]?),(?'ZMAX1'[01]?),(?'ZMIN2'[01]?),(?'ZMAX2'[01]?)>");

        private static Regex LagoVistaErrorRegEx = new Regex(@"<(?'State'Alarm|Message|EndStop)?:(?'Msg'[\w]*)>");

        private static Regex LagoVistaAccStatus = new Regex(@"<tl:(?'topLight'[01]),bl:(?'bottomLight'[01]),v1:(?'vacuum1'[01]),v2:(?'vacuum2'[01]),s1:(?'solenoid'[01]),t:(?'tool'[01])>");

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

        int _currentTool;

        public bool ParseLagoVistaLine(String line)
        {
            var lgvStatusMatch1 = LagoVistaStatusRegEx1.Match(line);
            var lgvStatusMatch2 = LagoVistaStatusRegEx2.Match(line);
            var lgvStatusMatch3 = LagoVistaStatusRegEx3.Match(line);
            var lgvErrorMatch = LagoVistaErrorRegEx.Match(line);
            var endStopMessage = LagoVistaEndStop.Match(line);
            var accMessage = LagoVistaAccStatus.Match(line);
            var lgvMovementMode = LagoVistaMovementMode.Match(line);

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
                    mt2 = lgvStatusMatch1.Groups["MT2"],
                    vt = lgvStatusMatch1.Groups["VT"];

                var newMachinePosition = new Vector3(double.Parse(mx.Value, Constants.DecimalParseFormat), double.Parse(my.Value, Constants.DecimalParseFormat), 0);

                if (MachinePosition != newMachinePosition)
                {
                    MachinePosition = newMachinePosition;
                }

                switch(vt.Value)
                {
                    case "camera": SetViewType(ViewTypes.Camera); break;
                    case "tool1": SetViewType(ViewTypes.Tool1); break;
                    case "tool2": SetViewType(ViewTypes.Tool2); break;
                }

                Tool0 = double.Parse(mt0.Value, Constants.DecimalParseFormat);
                Tool1 = double.Parse(mt1.Value, Constants.DecimalParseFormat);
                Tool2 = double.Parse(mt2.Value, Constants.DecimalParseFormat);

                return true;
            }
            else if (accMessage.Success)
            {
                AddStatusMessage(StatusMessageTypes.ReceivedLine, line, MessageVerbosityLevels.Normal);

                _bottomLightOn = accMessage.Groups["bottomLight"].Value == "1";
                _topLightOn = accMessage.Groups["topLight"].Value == "1";
                _vacuum1On = accMessage.Groups["vacuum1"].Value == "1";
                _vacuum2On = accMessage.Groups["vacuum2"].Value == "1";
                _solendoidOn = accMessage.Groups["solenoid"].Value == "1";

                RaisePropertyChanged(nameof(SolendoidOn));
                RaisePropertyChanged(nameof(Vacuum1On));
                RaisePropertyChanged(nameof(Vacuum2On));
                RaisePropertyChanged(nameof(BottomLightOn));
                RaisePropertyChanged(nameof(TopLightOn));
                
                return true;
            }
            else if (lgvMovementMode.Success)
            {
                AddStatusMessage(StatusMessageTypes.ReceivedLine, line, MessageVerbosityLevels.Normal);
                DistanceMode = lgvMovementMode.Groups["mode"].Value == "absolute" ? Core.GCode.ParseDistanceMode.Absolute : Core.GCode.ParseDistanceMode.Relative;

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

                var newWorkPosition = new Vector3(double.Parse(wx.Value, Constants.DecimalParseFormat), double.Parse(wy.Value, Constants.DecimalParseFormat), double.Parse(_currentTool == 0 ? wt0.Value : wt1.Value, Constants.DecimalParseFormat));

                if (WorkPositionOffset != newWorkPosition)
                {
                    WorkPositionOffset = newWorkPosition;
                }
                return true;
            }
            else if (lgvStatusMatch3.Success)
            {
                Group t = lgvStatusMatch3.Groups["TO"], p = lgvStatusMatch3.Groups["PA"];
                Group tl = lgvStatusMatch3.Groups["TL"], bl = lgvStatusMatch3.Groups["BL"], va = lgvStatusMatch3.Groups["VA"], su = lgvStatusMatch3.Groups["SU"], ex = lgvStatusMatch3.Groups["EX"];

                _currentTool = int.Parse(t.Value);
            }
            else if (lgvErrorMatch.Success)
            {
                Group state = lgvErrorMatch.Groups["State"], msg = lgvErrorMatch.Groups["Msg"];
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
                var endStops = new Dictionary<string, Group>();

                endStops.Add("XMIN", endStopMessage.Groups["XMIN"] as Group);
                endStops.Add("XMAX", endStopMessage.Groups["XMAX"] as Group);

                endStops.Add("YMIN", endStopMessage.Groups["YMIN"] as Group);
                endStops.Add("YMAX", endStopMessage.Groups["YMAX"] as Group);

                endStops.Add("ZMIN1", endStopMessage.Groups["ZMIN1"] as Group);
                endStops.Add("ZMAX1", endStopMessage.Groups["ZMAX1"] as Group);

                endStops.Add("ZMIN2", endStopMessage.Groups["ZMIN2"] as Group);
                endStops.Add("ZMAX2", endStopMessage.Groups["ZMAX2"] as Group);

                foreach (var endStop in endStops.Keys)
                {
                    var endStopStatus = endStops[endStop];
                    if (endStopStatus.Success)
                    {
                        if (endStopStatus.Value == "1")
                        {
                            AddStatusMessage(StatusMessageTypes.FatalError, "Endstop Hit: " + endStop);
                        }

                        bool? value = null;
                        if (!String.IsNullOrEmpty(endStopStatus.Value))
                            value = endStopStatus.Value == "1";

                        switch (endStop)
                        {
                            case "XMIN": EndStopXMin = value; break;
                            case "XMAX": EndStopXMax = value; break;
                            case "YMIN": EndStopYMin = value; break;
                            case "YMAX": EndStopYMax = value; break;
                            case "ZMIN1": EndStopZ1Min = value; break;
                            case "ZMAX1": EndStopZ1Max = value; break;
                            case "ZMIN2": EndStopZ2Min = value; break;
                            case "ZMAX2": EndStopZ2Max = value; break;
                        }
                    }
                }

                return true;
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
