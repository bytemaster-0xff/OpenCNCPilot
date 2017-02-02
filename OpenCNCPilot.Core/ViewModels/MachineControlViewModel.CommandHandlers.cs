
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
            switch (direction)
            {

            }
        }

        public void ResetAxis(ResetAxis axis)
        {
            /*var command  = $"G10 L2 P0 X{_machine.WorkPosition.X.ToString(Constants.DecimalOutputFormat)} Y{_machine.WorkPosition.Y.ToString(Constants.DecimalOutputFormat)} Z{_machine.WorkPosition.Z.ToString(Constants.DecimalOutputFormat)}";
            var cmd2  = "G92 X0 Y0 Z0";
            var cmd3  = "G10 L2 P0 X0 Y0 Z0";*/
        }
    }
}
