using LagoVista.Core;
using LagoVista.Core.GCode.Commands;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MachineControlViewModel
    {
        public void CycleStart()
        {
            Machine.CycleStart();
        }

        public void SoftReset()
        {
            Machine.SoftReset();
        }

        public void FeedHold()
        {
            Machine.FeedHold();
        }

        public void ClearAlarm()
        {
            Machine.ClearAlarm();
        }

        private void RelativeJog(JogDirections direction)
        {
            Machine.SendCommand("G91");

            switch (direction)
            {
                case JogDirections.XPlus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} X{XYStepSize.ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.YPlus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Y{(XYStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.ZPlus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.XMinus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} X{(-XYStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.YMinus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Y{(-XYStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.ZMinus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(-ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T0Minus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(-ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T0Plus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T1Minus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(-ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T1Plus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
            }

            Machine.SendCommand("G90");

        }

        private void AbsoluteJog(JogDirections direction)
        {
            var current = Machine.WorkspacePosition;

            switch (direction)
            {
                case JogDirections.XPlus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} X{(current.X + XYStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.YPlus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Y{(current.Y + XYStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.ZPlus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(current.Z + ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.XMinus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} X{(current.X - XYStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.YMinus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Y{(current.Y - XYStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.ZMinus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(current.Z - ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T0Minus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(Machine.Tool0 - ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T0Plus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(Machine.Tool0 + ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T1Minus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(Machine.Tool1 - ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T1Plus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(Machine.Tool1 + ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.CMinus:
                    {
                        var newAngle = Machine.Tool2 - 90;
                        if (newAngle > 360)
                            newAngle -= 360;

                        if(newAngle < 0)
                        {
                            newAngle += 360;
                        }

                        Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} E{(newAngle).ToDim()} F5000");
                    }
                    break;

                case JogDirections.CPlus:
                    {
                        var newAngle = Machine.Tool2 + 90;

                        if (newAngle > 360)
                            newAngle -= 360;

                        if (newAngle < 0)
                        {
                            newAngle += 360;
                        }

                        Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} E{(newAngle).ToDim()} F5000"); break;
                    }
            }
        }

        public void Jog(JogDirections direction)
        {
            if((Machine.Settings.MachineType == FirmwareTypes.Repeteir_PnP && 
                direction != JogDirections.CMinus && direction != JogDirections.CPlus) ||
                Machine.Settings.MachineType == FirmwareTypes.GRBL1_1)
            {
                RelativeJog(direction);
            }
            else
            {
                AbsoluteJog(direction);
            }
        }

        public void Home(HomeAxis axis)
        {
            switch (axis)
            {
                case Sender.HomeAxis.All:
                    Machine.SendCommand("G28 X Y");
                    Machine.SendCommand("T0");
                    Machine.SendCommand("G28 Z");
                    Machine.SendCommand("T1");
                    Machine.SendCommand("G28 Z");
                    Machine.SendCommand("T1");
                    Machine.SendCommand("G28 Z");
                    break;
                case Sender.HomeAxis.X:
                    Machine.SendCommand("G28 X");
                    break;
                case Sender.HomeAxis.Y:
                    Machine.SendCommand("G28 Y");
                    break;
                case Sender.HomeAxis.Z:
                    Machine.SendCommand("G28 Z");
                    break;
                case HomeAxis.T0:
                    Machine.SendCommand("G28 Z");
                    break;
                case HomeAxis.T1:
                    Machine.SendCommand("G28 P");
                    break;
                case HomeAxis.C:
                    Machine.SendCommand("G28 C");
                    break;
            }
        }

        public void ResetAxis(ResetAxis axis)
        {
            switch (axis)
            {
                case Sender.ResetAxis.All:
                    if (Machine.Settings.MachineType == FirmwareTypes.GRBL1_1)
                    {
                        Machine.SendCommand("G10 P0 L20 X0 Y0 Z0");
                    }
                    else
                    {
                        Machine.SendCommand("G10 L20 P0 X0 Y0 Z0 C0");
                    }
                    break;
                case Sender.ResetAxis.X:
                    Machine.SendCommand("G10 L20 P0 X0");
                    break;
                case Sender.ResetAxis.Y:
                    Machine.SendCommand("G10 L20 P0 Y0");
                    break;
                case Sender.ResetAxis.Z:
                    Machine.SendCommand("G10 L20 P0 Z0");
                    break;
                case Sender.ResetAxis.T0:
                    Machine.SendCommand("G10 L20 P0 Z0");
                    break;
                case Sender.ResetAxis.T1:
                    Machine.SendCommand("G10 L20 P0 Z0");
                    break;
                case Sender.ResetAxis.C:
                    Machine.SendCommand("G10 L20 P0 C0");
                    break;


            }
        }
    }
}