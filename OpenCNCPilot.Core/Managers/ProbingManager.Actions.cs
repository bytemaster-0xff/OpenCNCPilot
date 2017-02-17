using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class ProbingManager
    {
        private static Regex ProbeEx = new Regex(@"\[PRB:(?'MX'-?[0-9]+\.?[0-9]*),(?'MY'-?[0-9]+\.?[0-9]*),(?'MZ'-?[0-9]+\.?[0-9]*):(?'Success'0|1)\]");

        /// <summary>
        /// Parses a recevied probe report
        /// </summary>
        public Vector3? ParseProbeLine(string line)
        {
            Match probeMatch = ProbeEx.Match(line);
            Group mx = probeMatch.Groups["MX"];
            Group my = probeMatch.Groups["MY"];
            Group mz = probeMatch.Groups["MZ"];
            Group success = probeMatch.Groups["Success"];

            if (!probeMatch.Success || !(mx.Success & my.Success & mz.Success & success.Success))
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, $"Received Bad Probe: '{line}'");
                return null;
            }

            var probePos = new Vector3(double.Parse(mx.Value, Constants.DecimalParseFormat), double.Parse(my.Value, Constants.DecimalParseFormat), double.Parse(mz.Value, Constants.DecimalParseFormat));

            probePos += Machine.WorkPosition - Machine.MachinePosition;     //Mpos, Wpos only get updated by the same dispatcher, so this should be thread safe
            return probePos;
        }

        public void StartProbe()
        {

        }

        public void CancelProbe()
        {

        }

        public void SetZAxis()
        {

        }

        public void ProbeCompleted(Vector3 position)
        {
            throw new NotImplementedException();
        }

        public void ProbeFailed()
        {

        }
    }
}
