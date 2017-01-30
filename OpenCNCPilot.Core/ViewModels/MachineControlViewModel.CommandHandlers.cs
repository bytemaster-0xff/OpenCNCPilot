
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
            /*            switch (direction)
                        {

                        }*/
        }

        public void ResetAxis(Reset axis)
        {
            /*var command  = $"G10 L2 P0 X{_machine.WorkPosition.X.ToString(Constants.DecimalOutputFormat)} Y{_machine.WorkPosition.Y.ToString(Constants.DecimalOutputFormat)} Z{_machine.WorkPosition.Z.ToString(Constants.DecimalOutputFormat)}";
            var cmd2  = "G92 X0 Y0 Z0";
            var cmd3  = "G10 L2 P0 X0 Y0 Z0";*/
        }

        public void SetStepSize(Axis axis, MicroStepModes stepSize)
        {
            double maxStep = 0;
            double minStep = 0;

            switch (stepSize)
            {
                case MicroStepModes.Large:
                    maxStep = 20;
                    minStep = 1;
                    break;
                case MicroStepModes.Medium:
                    maxStep = 10;
                    minStep = 1;
                    break;
                case MicroStepModes.Small:
                    maxStep = 1;
                    minStep = 0.1;
                    break;
                case MicroStepModes.Micro:
                    maxStep = 0.1;
                    minStep = 0.01;
                    break;
            }


            if (axis == Axis.XY)
            {
                XYStepMin = minStep;
                XYStepMax = maxStep;
                XYStep = (minStep + maxStep) / 2;
            }
            else
            {
                ZStepMin = minStep;
                ZStepMax = maxStep;
                ZStep = (minStep + maxStep) / 2;
            }
        }
    }
}
