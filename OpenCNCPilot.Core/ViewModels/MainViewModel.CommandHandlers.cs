using LagoVista.Core.GCode;
using LagoVista.GCode.Sender.Models;
using System;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MainViewModel
    {

        public void SetAbsolutePositionMode()
        {
            AssertInManualMode(() => Machine.SendCommand("G90"));
        }

        public void SetIncrementalPositionMode()
        {
            AssertInManualMode(() => Machine.SendCommand("G91"));
        }

        private void ButtonArcPlane_Click()
        {
            if (Machine.Mode != OperatingMode.Manual)
                return;

            if (Machine.Plane != ArcPlane.XY)
                Machine.SendCommand("G17");
        }

        //http://www.cnccookbook.com/CCCNCGCodeG20G21MetricImperialUnitConversion.htm
        public void SetImperialUnits()
        {
            AssertInManualMode(() => Machine.SendCommand("G20"));
        }

        public void SetMetricUnits()
        {
            AssertInManualMode(() => Machine.SendCommand("G21"));
        }


        public void ClearHeightMap()
        {
            HeightMap = null;
        }

        public async void ArcToLine()
        {
            if (Machine.JobManager.HasValidFile)
            {
                var result = await Popups.PromptForDoubleAsync("Convert Line to Arch", Machine.Settings.ArcToLineSegmentLength, "Enter Arc Width", true);
                if (result.HasValue)
                    Machine.JobManager.ArcToLines(result.Value);
            }
        }

        public void ApplyHeightMap()
        {
            if (Machine.JobManager.HasValidFile && HeightMap != null)
            {
                Machine.JobManager.ApplyHeightMap(HeightMap);
            }
        }

        public async void OpenGCodeFile(object instance)
        {
            var file = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!String.IsNullOrEmpty(file))
            {
                await Machine.JobManager.OpenFileAsync(file); 
            }
        }

        public async void OpenHeightMapFile(object instance)
        {
            var heightMap = await HeightMap.OpenAsync(Machine.Settings);
            if (heightMap != null)
            {
                HeightMap = heightMap;
            }
        }

        public void CloseFile(object instance)
        {
            Machine.JobManager.CloseFileAsync();
        }
    }
}
