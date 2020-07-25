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
        /// <summary>
        /// As commands are sent to the machine the number of bytes for that command are added to 
        /// the UnacknowledgedByteSet property, once the command has been acknowledged the number 
        /// of bytes for that acknowledged command will be subtracted.  This will keep a rough idea 
        /// of the number of bytes that have been buffered on the machine.  The size of the buffer
        /// on the machine has been entered in settings, thus we know the available space and can
        /// send additional bytes for future commands and have them ready for the machine.  These
        /// can then be queued so the machine doesn't have to wait for the next communications.
        /// </summary>
        int UnacknowledgedBytesSent { get; }

        /// <summary>
        /// Perform any additional tasks to initialize the machine, should be called as soon 
        /// as possible
        /// </summary>
        /// <returns></returns>
        Task InitAsync();

        /// <summary>
        /// Will be set as soon as machine initialization has been completed.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// The current XYZ position of the machien with respect to the specified origin of the physical machine (0,0,0)
        /// </summary>
        Vector3 MachinePosition { get; }

        /// <summary>
        /// The current XYZ position of the machine with respect to the resetted home position of the work.
        /// </summary>
        Vector3 WorkPositionOffset { get; }

        Vector3 NormalizedPosition{ get; }

        double Tool0 { get;  }
        double Tool1 { get; }
        double Tool2 { get; }


        double Tool0Offset { get; }
        double Tool1Offset { get; }
        double Tool2Offset { get; }


        double Tool0Normalized { get; }
        double Tool1Normalized { get; }
        double Tool2Normalized { get; }


        bool TopLightOn { get; set; }
        bool BottomLightOn { get; set; }
        bool Vacuum1On { get; set; }
        bool Vacuum2On { get; set; }
        bool SolendoidOn { get; set; }

        /// <summary>
        /// Current mode of the machine, such as Connected, Running a Job, etc....
        /// </summary>
        OperatingMode Mode { get; }

        /// <summary>
        /// Total number of messages in the message list to be displayed, primarily used to scroll the list.
        /// </summary>
        int MessageCount { get; }

        /// <summary>
        /// Type of view such as camera, tool1, tool2
        /// </summary>
        ViewTypes ViewType { get; set; }

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
        /// Status as reported by machine.
        /// </summary>
        String Status { get; }

        /// <summary>
        /// Business logic to manage the sending of GCode files to the machine.
        /// </summary>
        IGCodeFileManager GCodeFileManager { get; }

        /// <summary>
        /// Business logic to handle working with Printed Circut Boards
        /// </summary>
        IPCBManager PCBManager { get; }

        /// <summary>
        /// Business logic for capturing a height map that can be applied to a GCode file to correct for warpage of material
        /// </summary>
        IHeightMapManager HeightMapManager { get; }

        /// <summary>
        /// Business logic for probe function to find the ZAxis where it comes in contact with the material to be machined.
        /// </summary>
        IProbingManager ProbingManager { get; }

        /// <summary>
        /// Business logic to accurately determine the board position and alignment using Machine Vision.
        /// </summary>
        IBoardAlignmentManager BoardAlignmentManager { get; }

        MachinesRepo MachineRepo { get; }


        bool Busy { get; }

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
        /// Connect to a remote machine via a socket.
        /// </summary>
        /// <param name="socketClient"></param>
        /// <returns></returns>
        Task ConnectAsync(ISocketClient socketClient);

        /// <summary>
        /// Disconnect from the machien
        /// </summary>
        /// <returns></returns>
        Task DisconnectAsync();

        bool MotorsEnabled { get; set; }
        
        /// <summary>
        /// Perform a soft reset
        /// </summary>
        void SoftReset();

        void FeedHold();

        void CycleStart();

        void ClearAlarm();

        void HomingCycle();

        void SetFavorite1();

        void SetFavorite2();

        void GotoFavorite1();

        void GotoFavorite2();

        void SpindleOn();
        void SpindleOff();

        void LaserOn();

        void LaserOff();

        void GotoPoint(Point2D<double> point, bool rapidMove = true);

        void GotoPoint(double x, double y, bool rapidMove = true);

        void GotoPoint(double x, double y, double z, bool rapidMove = true);

        void GotoWorkspaceHome();

        void GotoFiducialHome();

        /// <summary>
        /// Send a message to the machine to immediately stop any motion operation
        /// </summary>
        void EmergencyStop();

        /// <summary>
        /// This method can be called to ensure that the machine can transition to the specified operating mode, if it can't a message will be added to the output and false will be returned.
        /// </summary>
        /// <param name="mode">The new desired transition mode.</param>
        /// <returns>True if you can transition into the mode, false if you can not.</returns>
        bool CanSetMode(OperatingMode mode);
    
        /// <summary>
        /// Transition the machine to the new mode.
        /// </summary>
        /// <param name="mode">The new mode</param>
        /// <returns>True if you can transition occurred, false if it did not, if it did not a warning message will be written to the message output.</returns>
        bool SetMode(OperatingMode mode);
        
        /// <summary>
        /// Determine if there are enough bytes in the estimated machine buffer to send the next command based on the bytes required to send that command
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>True if there is space available to send bytes</returns>
        bool HasBufferSpaceAvailableForByteCount(int bytes);

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
        MachineSettings Settings { get; set; }
    }
}
