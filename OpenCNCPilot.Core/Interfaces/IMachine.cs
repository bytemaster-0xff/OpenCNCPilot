using LagoVista.Core.GCode;
using LagoVista.Core.GCode.Commands;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Interfaces
{
    public interface IMachine : INotifyPropertyChanged
    {
        event Action<Vector3, bool> ProbeFinished;


        /// <summary>
        /// As commands are sent to the machine the number of bytes for that command are added to 
        /// the UnacknowledgedByteSet property, once the command has been acknowledged the number 
        /// of bytes for that acknowledged command will be subtracted.  This will keep a rough idea 
        /// of the number of bytes that have been buffered on the machine.  The size of the buffer
        /// on the machine has been entered in settings, thus we know the available space and can
        /// send additional bytes for future commands and have them ready for the machine.  These
        /// can then be queued so the machine doesn't have to wait for the next communications.
        /// </summary>
        int UnacknowledgedBytesSent { get; set; }

        /// <summary>
        /// Perform any additional tasks to initialize the machine, should be called as soon 
        /// as possible
        /// </summary>
        /// <returns></returns>
        Task InitAsync();

        /// <summary>
        /// Will be set as soon as machine initialization has been completed.
        /// </summary>
        bool IsInitialized { get; set; }

        /// <summary>
        /// The current XYZ position of the machien with respect to the specified origin of the physical machine (0,0,0)
        /// </summary>
        Vector3 MachinePosition { get; }

        /// <summary>
        /// The current XYZ position of the machine with respect to the resetted home position of the work.
        /// </summary>
        Vector3 WorkPosition { get; }

        /// <summary>
        /// Current mode of the machine, such as Connected, Running a Job, etc....
        /// </summary>
        OperatingMode Mode { get; }

        /// <summary>
        /// Total number of messages in the message list to be displayed, primarily used to scroll the list.
        /// </summary>
        int MessageCount { get; }

        /// <summary>
        /// Mode in which GCode commands should be interpretted.  These are either absolute with repsect to
        /// the origin, or incremenental which should be added to the current position.
        /// </summary>
        ParseDistanceMode DistanceMode { get;  }

        /// <summary>
        /// If the units are to be sent as inches or millimeters
        /// </summary>
        ParseUnit Unit { get; }

        /// <summary>
        /// Arch Plene, have to admit, I don't understand this yet...from original OpenCNCPilot, I'm 100% positive it's needed at some point though
        /// </summary>
        ArcPlane Plane { get; }

        /// <summary>
        /// Business logic to manage the sending of GCode files to the machien.
        /// </summary>
        IJobManager JobManager { get; }

        /// <summary>
        /// Business logic for capturing a height map that can be applied to a GCode file to correct for warpage of material
        /// </summary>
        IHeightMapProbingManager HeightMapProbingManager { get; }

        /// <summary>
        /// Business logic for probe function to find the ZAxis where it comes in contact with the material to be machined.
        /// </summary>
        IProbingManager ProbingManager { get; }

        /// <summary>
        /// Is the machine currently connected
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Connect to the machine
        /// </summary>
        /// <param name="serialPort"></param>
        /// <returns></returns>
        Task ConnectAsync(ISerialPort serialPort);

        /// <summary>
        /// Disconnect from the machien
        /// </summary>
        /// <returns></returns>
        Task DisconnectAsync();
        
        /// <summary>
        /// Perform a soft reset
        /// </summary>
        void SoftReset();

        void FeedHold();

        void CycleStart();

        void ClearAlarm();


        /// <summary>
        /// Determine if there are enough bytes in the estimated machine buffer to send the next command based on the bytes required to send that command
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        bool BufferSpaceAvailable(int bytes);

        /// <summary>
        /// Send a GCode Command to the machine
        /// </summary>
        /// <param name="cmd">A paresed GCode line from a file to be sent to the machine</param>
        void SendCommand(GCodeCommand cmd);

        /// <summary>
        /// Send a free form comamdn to the machine.
        /// </summary>
        /// <param name="cmd">Text that represents the command</param>
        void SendCommand(String cmd);

        /// <summary>
        /// Add a message to be displayed to the user.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        void AddStatusMessage(StatusMessageTypes type, String message, MessageVerbosityLevels verbosity = MessageVerbosityLevels.Normal);

        /// <summary>
        /// Current settings as to be used by the machine.
        /// </summary>
        Settings Settings { get; }
    }
}
