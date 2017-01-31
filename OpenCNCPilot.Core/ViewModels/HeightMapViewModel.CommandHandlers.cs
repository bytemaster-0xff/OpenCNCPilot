using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class HeightMapViewModel
    {
        private void HeightMapProbeNextPoint()
        {
           
            if (Machine.Mode != OperatingMode.ProbingHeightMap)
                return;

            if (!Machine.Connected || _currentHeightMap == null || _currentHeightMap.NotProbed.Count == 0)
            {
                Machine.ProbeStop();
                return;
            }

            var nextPoint = _currentHeightMap.GetCoordinates(_currentHeightMap.NotProbed.Peek().Item1, _currentHeightMap.NotProbed.Peek().Item2);

            Machine.SendLine($"G0X{nextPoint.X.ToString("0.###", Constants.DecimalOutputFormat)}Y{nextPoint.Y.ToString("0.###", Constants.DecimalOutputFormat)}");

            Machine.SendLine($"G38.3Z-{Settings.ProbeMaxDepth.ToString("0.###", Constants.DecimalOutputFormat)}F{Settings.ProbeFeed.ToString("0.#", Constants.DecimalOutputFormat)}");

            Machine.SendLine("G91");
            Machine.SendLine($"G0Z{Settings.ProbeMinimumHeight.ToString("0.###", Constants.DecimalOutputFormat)}");
            Machine.SendLine("G90");
        }

        public void ProbingFinished(Vector3 position, bool success)
        {
            if (Machine.Mode != OperatingMode.ProbingHeightMap)
                return;

            if (!Machine.Connected || _currentHeightMap == null || _currentHeightMap.NotProbed.Count == 0)
            {
                Machine.ProbeStop();
                return;
            }

            if (!success && Settings.AbortOnProbeFail)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Probe Failed! aborting");
                Machine.ProbeStop();
                return;
            }

            Tuple<int, int> lastPoint = _currentHeightMap.NotProbed.Dequeue();

            _currentHeightMap.AddPoint(lastPoint.Item1, lastPoint.Item2, position.Z);

            if (_currentHeightMap.NotProbed.Count == 0)
            {
                Machine.SendLine($"G0Z{Settings.ProbeSafeHeight.ToString(Constants.DecimalOutputFormat)}");
                Machine.ProbeStop();
                return;
            }

            HeightMapProbeNextPoint();
        }

        public void StartProbing()
        {
            if (!Machine.Connected || Machine.Mode != OperatingMode.Manual || _currentHeightMap == null)
                return;

            if (_currentHeightMap.Progress == _currentHeightMap.TotalPoints)
                return;

            Machine.ProbeStart();

            if (Machine.Mode != OperatingMode.ProbingHeightMap)
                return;

            Machine.SendLine("G90");
            Machine.SendLine($"G0Z{Settings.ProbeSafeHeight.ToString("0.###", Constants.DecimalOutputFormat)}");

            HeightMapProbeNextPoint();
        }

        public void PauseProbing()
        {
            if (Machine.Mode != OperatingMode.ProbingHeightMap)
                return;

            Machine.ProbeStop();
        }

        public void HeightMapChanged()
        {
            
        }
    }
}
