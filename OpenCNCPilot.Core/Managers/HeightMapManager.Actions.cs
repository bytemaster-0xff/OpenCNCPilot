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

            if (!Machine.Connected || HeightMap == null || HeightMap.NotProbed.Count == 0)
            {
                CancelProbing();
                return;
            }

            var lastPoint = HeightMap.NotProbed.Dequeue();

            HeightMap.AddPoint(lastPoint.Item1, lastPoint.Item2, position.Z);

            if (HeightMap.NotProbed.Count == 0)
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
            HeightMap = new Models.HeightMap();
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

        private void HeightMapProbeNextPoint()
        {

            if (Machine.Mode != OperatingMode.ProbingHeightMap)
                return;

            if (!Machine.Connected || HeightMap == null || HeightMap.NotProbed.Count == 0)
            {
                CancelProbing();
                return;
            }

            var nextPoint = HeightMap.GetCoordinates(HeightMap.NotProbed.Peek().Item1, HeightMap.NotProbed.Peek().Item2);

            Machine.SendCommand($"G0X{nextPoint.X.ToString("0.###", Constants.DecimalOutputFormat)}Y{nextPoint.Y.ToString("0.###", Constants.DecimalOutputFormat)}");

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
