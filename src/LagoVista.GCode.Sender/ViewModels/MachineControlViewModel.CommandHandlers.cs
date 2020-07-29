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

        public void Jog(JogDirections direction)
        {
            var current = (Machine.Settings.MachineType == FirmwareTypes.GRBL1_1 || Machine.Settings.MachineType == FirmwareTypes.LagoVista || Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP) ? Machine.MachinePosition - Machine.WorkPositionOffset : Machine.MachinePosition;
            var currentTool0 = Machine.Settings.MachineType == FirmwareTypes.GRBL1_1 || (Machine.Settings.MachineType == FirmwareTypes.LagoVista || Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP) ? Machine.Tool0 - Machine.Tool0Offset : Machine.Tool0;
            var currentTool1 = Machine.Settings.MachineType == FirmwareTypes.GRBL1_1 || (Machine.Settings.MachineType == FirmwareTypes.LagoVista || Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP) ? Machine.Tool1 - Machine.Tool1Offset : Machine.Tool1;
            var currentTool2 = Machine.Settings.MachineType == FirmwareTypes.GRBL1_1 || (Machine.Settings.MachineType == FirmwareTypes.LagoVista || Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP) ? Machine.Tool2 - Machine.Tool2Offset : Machine.Tool2;


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
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(currentTool0 - ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T0Plus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(currentTool0 + ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T1Minus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(currentTool1 - ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T1Plus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(currentTool1 + ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T2Minus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(currentTool2 - ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T2Plus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{(currentTool2 + ZStepSize).ToDim()} F{Machine.Settings.JogFeedRate}");
                    break;
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
                    Machine.SendCommand("T0");
                    Machine.SendCommand("G28 Z");
                    break;
                case HomeAxis.T1:
                    Machine.SendCommand("T1");
                    Machine.SendCommand("G28 Z");
                    break;
                case HomeAxis.T2:
                    Machine.SendCommand("T2");
                    Machine.SendCommand("G0 Z0");
                    break;
            }
        }

        public void ResetAxis(ResetAxis axis)
        {
            switch (axis)
            {
                case Sender.ResetAxis.All:
                    Machine.SendCommand("G10 L20 P0 X0 Y0 Z0");
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
                    Machine.SendCommand("T0");
                    Machine.SendCommand("G10 L20 P0 Z0");
                    break;
                case Sender.ResetAxis.T1:
                    Machine.SendCommand("T1");
                    Machine.SendCommand("G10 L20 P0 Z0");
                    break;
                case Sender.ResetAxis.T2:
                    Machine.SendCommand("T1");
                    Machine.SendCommand("G10 L20 P0 Z0");
                    break;


            }
        }
    }
}