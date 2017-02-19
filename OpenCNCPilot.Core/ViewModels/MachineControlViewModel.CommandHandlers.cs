
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
            var current = Machine.MachinePosition - Machine.WorkPosition;

            switch (direction)
            {
                case JogDirections.XPlus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} X{current.X + XYStepSize} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.YPlus:
                    Machine.SendCommand( $"{Machine.Settings.JogGCodeCommand} Y{current.Y + XYStepSize} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.ZPlus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{current.Z + ZStepSize} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.XMinus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} X{current.X - XYStepSize} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.YMinus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Y{current.Y - XYStepSize} F{Machine.Settings.JogFeedRate}");
                    break;
                case JogDirections.ZMinus:
                    Machine.SendCommand($"{Machine.Settings.JogGCodeCommand} Z{current.Z - ZStepSize} F{Machine.Settings.JogFeedRate}");
                    break;
            }
        }

        public void ResetAxis(ResetAxis axis)
        {
            switch(axis)
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

            }
            /*var command  = $"G10 L2 P0 X{_machine.WorkPosition.X.ToString(Constants.DecimalOutputFormat)} Y{_machine.WorkPosition.Y.ToString(Constants.DecimalOutputFormat)} Z{_machine.WorkPosition.Z.ToString(Constants.DecimalOutputFormat)}";
            var cmd2  = "G92 X0 Y0 Z0";
            var cmd3  = "G10 L2 P0 X0 Y0 Z0";*/
        }
    }
}