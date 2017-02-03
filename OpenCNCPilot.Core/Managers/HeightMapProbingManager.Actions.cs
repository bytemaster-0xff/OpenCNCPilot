using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class HeightMapProbingManager
    {
        public void ProbingFinished(Vector3 position, bool success)
        {
            if (Machine.Mode != OperatingMode.ProbingHeightMap)
                return;

            if (!Machine.Connected || HeightMap == null || HeightMap.NotProbed.Count == 0)
            {
                Machine.ProbeStop();
                return;
            }

            if (!success && Machine.Settings.AbortOnProbeFail)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Probe Failed! aborting");
                Machine.ProbeStop();
                return;
            }

            Tuple<int, int> lastPoint = HeightMap.NotProbed.Dequeue();

            HeightMap.AddPoint(lastPoint.Item1, lastPoint.Item2, position.Z);

            if (HeightMap.NotProbed.Count == 0)
            {
                Machine.SendCommand($"G0Z{Machine.Settings.ProbeSafeHeight.ToString(Constants.DecimalOutputFormat)}");
                Machine.ProbeStop();

                ProbingCompleted?.Invoke(this, null);
                return;
            }

            HeightMapProbeNextPoint();
        }


        private void HeightMapProbeNextPoint()
        {

            if (Machine.Mode != OperatingMode.ProbingHeightMap)
                return;

            if (!Machine.Connected || HeightMap == null || HeightMap.NotProbed.Count == 0)
            {
                Machine.ProbeStop();
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

            Machine.ProbeStart();

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
          
            Machine.ProbeStop();
        }
    }
}
