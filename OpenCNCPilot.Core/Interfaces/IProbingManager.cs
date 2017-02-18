using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using System.ComponentModel;

namespace LagoVista.GCode.Sender.Interfaces
{
    public interface IProbingManager : INotifyPropertyChanged
    {
        /// <summary>
        /// Begin the process that will be used to set the machine height to the top of the material.
        /// </summary>
        void StartProbe();

        /// <summary>
        /// Abort the process to probe for material height.
        /// </summary>
        void CancelProbe();

        /// <summary>
        /// Probe was completed.
        /// </summary>
        /// <param name="probeResult"></param>
        void ProbeCompleted(Vector3 probeResult);

        /// <summary>
        /// Probing failed.
        /// </summary>
        void ProbeFailed();

        /// <summary>
        /// Send a command to the machine to set the new Z-Axis based on completion of probing
        /// </summary>
        void SetZAxis(double zOffset);

        /// <summary>
        /// Utility method to read in a line of text from machine and return probe information.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        Vector3? ParseProbeLine(string line);

        
    }
}
