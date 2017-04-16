using LagoVista.Core.GCode;
using LagoVista.EaglePCB.Managers;
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

        public async void OpenEagleBoardFile()
        {
            var file = await Popups.ShowOpenFileAsync(Constants.FileFilterPCB);
            if (!String.IsNullOrEmpty(file))
            {
                if(await Machine.PCBManager.OpenFileAsync(file))
                {
                    AddBoardFileMRU(file);
                }

            }
         }

        public void CloseEagleBoardFile()
        {

        }


        public void ClearHeightMap()
        {
            Machine.HeightMapManager.CloseHeightMap();
        }

        public async void ArcToLine()
        {
            if (Machine.GCodeFileManager.HasValidFile)
            {
                var result = await Popups.PromptForDoubleAsync("Convert Line to Arch", Machine.Settings.ArcToLineSegmentLength, "Enter Arc Width", true);
                if (result.HasValue)
                    Machine.GCodeFileManager.ArcToLines(result.Value);
            }
        }


        public async void OpenGCodeFile(object instance)
        {
            var file = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!String.IsNullOrEmpty(file))
            {
                if(await Machine.GCodeFileManager.OpenFileAsync(file))
                {
                    AddGCodeFileMRU(file);
                }
            }
        }


        public void CloseFile(object instance)
        {
            Machine.GCodeFileManager.CloseFileAsync();
        }

        public void ProbeHeightMap()
        {

        }

        public async void OpenHeightMapFile(object instance)
        {
            var file = await Popups.ShowOpenFileAsync(Constants.FileFilterHeightMap);
            if (!String.IsNullOrEmpty(file))
            {
                await Machine.HeightMapManager.OpenHeightMapAsync(file);
            }
        }

        public async void SaveHeightMap()
        {
            var file = await Popups.ShowSaveFileAsync(Constants.FileFilterHeightMap);
            if (!String.IsNullOrEmpty(file))
            {
                await Machine.HeightMapManager.SaveHeightMapAsync(file);
            }
        }

        public void ApplyHeightMap()
        {
            if (CanApplyHeightMap())
            {
                Machine.GCodeFileManager.ApplyHeightMap(Machine.HeightMapManager.HeightMap);
            }
        }

        public void SaveModifiedGCode()
        {

        }

        public async void GenerateHoldDownGCode()
        {
            var drillIntoUnderlayment = await Popups.ConfirmAsync("Drill Holes In Underlayment?", "Would you also like to drill the holes in the underlayment?  You only need to use this once when setting up a fixture.  After that you should use the holes that were already created.");
            Machine.GCodeFileManager.SetGCode(GCodeEngine.CreateHoldDownGCode(Machine.PCBManager.Board, Machine.PCBManager.Project, drillIntoUnderlayment));
        }

        public void GenerateMillingGCode()
        {
            Machine.GCodeFileManager.SetGCode(GCodeEngine.CreateCutoutMill(Machine.PCBManager.Board, Machine.PCBManager.Project));
        }

        public void GenerateDrillGCode()
        {
            Machine.GCodeFileManager.SetGCode(GCodeEngine.CreateDrillGCode(Machine.PCBManager.Board, Machine.PCBManager.Project));
        }

        public void ShowTopEtchingGCode()
        {
            Machine.GCodeFileManager.OpenFileAsync(Machine.PCBManager.Project.TopEtchingFilePath);
            Machine.GCodeFileManager.ApplyOffset(Machine.PCBManager.Project.ScrapSides, Machine.PCBManager.Project.ScrapTopBottom, 0);
        }

        public void ShowBottomEtchingGCode()
        {
            Machine.GCodeFileManager.OpenFileAsync(Machine.PCBManager.Project.BottomEtchingFilePath);
            Machine.GCodeFileManager.ApplyOffset((Machine.PCBManager.Project.ScrapSides) + Machine.PCBManager.Board.Width, Machine.PCBManager.Project.ScrapTopBottom, 0);
        }
    }
}
