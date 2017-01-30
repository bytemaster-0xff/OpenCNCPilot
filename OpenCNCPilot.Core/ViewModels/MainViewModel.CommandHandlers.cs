using LagoVista.Core.GCode;
using LagoVista.GCode.Sender.Models;
using System;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MainViewModel
    {

        public void SetAbsolutePositionMode()
        {
            AssertInManualMode(() => Machine.SendLine("G90"));
        }

        public void SetIncrementalPositionMode()
        {
            AssertInManualMode(() => Machine.SendLine("G91"));
        }

        private void ButtonArcPlane_Click()
        {
            if (Machine.Mode != OperatingMode.Manual)
                return;

            if (Machine.Plane != ArcPlane.XY)
                Machine.SendLine("G17");
        }

        //http://www.cnccookbook.com/CCCNCGCodeG20G21MetricImperialUnitConversion.htm
        public void SetImperialUnits()
        {
            AssertInManualMode(() => Machine.SendLine("G20"));
        }

        public void SetMetricUnits()
        {
            AssertInManualMode(() => Machine.SendLine("G21"));
        }


        public void ClearHeightMap()
        {
            HeightMapVM.CurrentHeightMap = null;
        }

        public async void ArcToLine()
        {
            if (Machine.HasJob)
            {
                var result = await Popups.PromptForDoubleAsync("Convert Line to Arch", Settings.ArcToLineSegmentLength, "Enter Arc Width", true);
                if (result.HasValue)
                    Machine.CurrentJob.ArcToLines(result.Value);
            }
        }

        public void ApplyHeightMap()
        {
            if (Machine.HasJob && HeightMapVM.CurrentHeightMap != null)
            {
                Machine.CurrentJob.ApplyHeightMap(HeightMapVM.CurrentHeightMap);
            }
        }

        public async void OpenGCodeFile(object instance)
        {
            var file = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!String.IsNullOrEmpty(file))
            {
                Machine.SetFile(GCodeFile.Load(file));
            }
        }

        public async void OpenHeightMapFile(object instance)
        {
            var heightMap = await HeightMap.OpenAsync(Settings);
            if (heightMap != null)
            {
                _heightMapVieModel.CurrentHeightMap = heightMap;
            }
        }

        public void CloseFile(object instance)
        {
            Machine.ClearFile();
        }
    }
}
