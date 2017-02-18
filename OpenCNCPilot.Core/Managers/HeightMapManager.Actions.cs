using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class HeightMapManager
    {
        public void ProbeCompleted(Vector3 position)
        {
            if (Machine.Mode != OperatingMode.ProbingHeightMap)
                return;

            if (!Machine.Connected || HeightMap == null)
            {
                CancelProbing();
                return;
            }

            if(HeightMap == null)
            {
                Logger.Log(Core.PlatformSupport.LogLevel.Error, "HeightMap_ProbeCompleted", "Probe Completed without valid Height Map.");
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Probe Height Map Completed without valid Height Map");
                CancelProbing();
                return;
            }

            if (_currentPoint == null)
            {
                Logger.Log(Core.PlatformSupport.LogLevel.Error, "HeightMap_ProbeCompleted", "Probe Completed without Current Point.");
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Probe Height Map Completed without valid Current Point");
                CancelProbing();
                return;
            }

            HeightMap.SetPointHeight(_currentPoint, position.Z);
            _currentPoint = null;

            if (HeightMap.Status == HeightMapStatus.Populated)
            {
                Machine.SendCommand($"G0Z{Machine.Settings.ProbeSafeHeight.ToString(Constants.DecimalOutputFormat)}");
                CancelProbing();

                return;
            }

            HeightMapProbeNextPoint();
        }

        public void NewHeightMap(HeightMap heightMap)
        {
            HeightMap = heightMap;

            ConstructVisuals();
        }

        private void ConstructVisuals()
        {
            if(!HasHeightMap)
            {
                Logger.Log(Core.PlatformSupport.LogLevel.Error, "HeightMapManager_ConstructVisuals", "Attempt to construct visual w/o a height map.");
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Attempt to construct visual w/o a height map.");
            }

            if (HeightMap.GridSize == 2.5)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, $"Grid size must be creater than 2.5, current grid size {HeightMap.GridSize}.");
                return;
            }

        }

        public async Task OpenHeightMapAsync(string path)
        {
            HeightMap = await Core.PlatformSupport.Services.Storage.GetAsync<HeightMap>(path);
        }

        public async Task SaveHeightMapAsync(string path)
        {
            await Core.PlatformSupport.Services.Storage.StoreAsync(HeightMap, path);
        }

        public async Task SaveHeightMapAsync()
        {
            var fileName = "foo";
            await Core.PlatformSupport.Services.Storage.StoreAsync(HeightMap, fileName);

        }

        public void CreateTestPattern()
        {
            HeightMap = new Models.HeightMap(Machine, Logger);
            HeightMap.FillWithTestPattern();
        }

        public void CloseHeightMap()
        {
            HeightMap = null;
        }

        public void ProbeFailed()
        {
            Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Probe Failed! aborting");
            CancelProbing();
        }

        public void ProbeStart()
        {

        }

        public void CancelProbing()
        {

        }

        HeightMapProbePoint _currentPoint;

        private void HeightMapProbeNextPoint()
        {
            if (Machine.Mode != OperatingMode.ProbingHeightMap)
                return;

            if (!Machine.Connected || HeightMap == null || HeightMap.Status == HeightMapStatus.Populated)
            {
                CancelProbing();
                return;
            }

            _currentPoint = HeightMap.GetNextPoint();

            Machine.SendCommand($"G0X{_currentPoint.Point.X.ToString("0.###", Constants.DecimalOutputFormat)}Y{_currentPoint.Point.Y.ToString("0.###", Constants.DecimalOutputFormat)}");

            Machine.SendCommand($"G38.3Z-{Machine.Settings.ProbeMaxDepth.ToString("0.###", Constants.DecimalOutputFormat)}F{Machine.Settings.ProbeFeed.ToString("0.#", Constants.DecimalOutputFormat)}");

            Machine.SendCommand("G91");
            Machine.SendCommand($"G0Z{Machine.Settings.ProbeMinimumHeight.ToString("0.###", Constants.DecimalOutputFormat)}");
            Machine.SendCommand("G90");
        }

        public void StartProbing()
        {
            if (!Machine.Connected || Machine.Mode != OperatingMode.Manual || HeightMap == null)
                return;

            if (HeightMap.Progress == HeightMap.TotalPoints)
                return;
            
            if (Machine.Mode != OperatingMode.ProbingHeightMap)
                return;

            Machine.SendCommand("G90");
            Machine.SendCommand($"G0Z{Machine.Settings.ProbeSafeHeight.ToString("0.###", Constants.DecimalOutputFormat)}");

            HeightMapProbeNextPoint();
        }

        public void PauseProbing()
        {
            if (Machine.Mode != OperatingMode.ProbingHeightMap)
                return;
       }
    }
}
