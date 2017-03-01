using LagoVista.Core.GCode.Commands;
using LagoVista.GCode.Sender.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Interfaces
{
    public interface IGCodeFileManager : INotifyPropertyChanged
    {
        /// <summary>
        /// Give a path, open a file that has GCode Commands
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task OpenFileAsync(string path);

        /// <summary>
        /// Close the current file.
        /// </summary>
        /// <returns></returns>
        Task CloseFileAsync();

        /// <summary>
        /// Total number of non-comment lines in the file.
        /// </summary>
        int TotalLines { get; }

        /// <summary>
        /// Called from the Machine as part of the work loop to see if there are any additional lines to send and if so send them.
        /// </summary>
        void ProcessNextLines();

        /// <summary>
        /// This method should be called when the machien receives an "OK" stating that a command sent has been acknowledged.
        /// </summary>
        /// <returns>Number of bytes for the command that has been acknowleged, this is used to update the estimated buffer size.</returns>
        int CommandAcknowledged();

        /// <summary>
        /// If a height map has been applied or arcs were converted to lines, the original GCode has been modifed.  Until the file has been saved the dirty flag will be set.
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// This gets set once the last item in the job has been acknowledged completed by the machine.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Pointer to the current item that the machine thinks is being processed.  Usually this will be the actual item but do to communication latency there maybe very small periods of time when the machine has moved on to the next command and the job processor has not been updated
        /// </summary>
        int Head { get; }

        /// <summary>
        /// Pointer to the last item that has been sent to the machine.
        /// </summary>
        int Tail { get; }

        /// <summary>
        /// Pointer to the currently executing command, alias for Head
        /// </summary>
        int CurrentIndex { get; }
        
        /// <summary>
        /// Time remaining for job completion
        /// </summary>
        TimeSpan EstimatedTimeRemaining { get; }

        /// <summary>
        /// The amount of time that the job has been running.
        /// </summary>
        TimeSpan ElapsedTime { get; }

        /// <summary>
        /// Estimatated tiem when this job should complete
        /// </summary>
        DateTime EstimatedCompletion { get; }


        LagoVista.Core.Models.Drawing.Point3D<double> Min { get; }
        LagoVista.Core.Models.Drawing.Point3D<double> Max { get; }

        /// <summary>
        /// Iterate through all the GCode commands and translate the points in the Z-Axis to take into account the height map.  Note: Credit needs to be give to the original OpenCNCPilot project, this code has been adopted.
        /// </summary>
        /// <param name="map"></param>
        void ApplyHeightMap(HeightMap map);

        /// <summary>
        /// Processes the file to turn any Arc GCode commands (G2, G3) to line commands (G0, G1) will not modify the source file but will allow the new file to be saved.  The job process will be marked as having a dirty file.
        /// </summary>
        /// <param name="length"></param>
        void ArcToLines(double length);

        /// <summary>
        /// Returns a full list of non comment commands that will be executed.
        /// </summary>
        IEnumerable<GCodeCommand> Commands { get; }

        /// <summary>
        /// The name of the current GCode File
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Returns true if there is a file available to be started.
        /// </summary>
        bool HasValidFile { get; }

        /// <summary>
        /// Command that the job process thinks the machien is executing.  This is the oldest command that was sent to the machine that has not been acknowledged.
        /// </summary>
        GCodeCommand CurrentCommand { get; }

        /// <summary>
        /// STart a job if one is ready
        /// </summary>
        void StartJob();

        /// <summary>
        /// Stops the current job on hold.  Note that command are already queued up on the machine so it will not stop immediately, also reset the pointers and indexes.
        /// </summary>
        void CancelJob();

        /// <summary>
        /// Pause the current job, the machine will continue to execute any commands that have currently been sent.  All the pointers and indexes are maitained so the job can be restarted.
        /// </summary>
        void PauseJob();

        /// <summary>
        /// Reset all the pointers and indexes for the current job, can only be done when the job is not running.  If job is paused it will stop the job.  This leaves the job in a state where it can be restarted from the beginning 
        /// </summary>
        void ResetJob();
    }
}
