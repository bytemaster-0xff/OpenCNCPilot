using OpenCNCPilot.Core.GCode;
using OpenCNCPilot.Core.GCode.GCodeCommands;
using OpenCNCPilot.Core.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCNCPilot.Core.Communication
{
    public interface IMachine : INotifyPropertyChanged
    {
        event Action<Vector3, bool> ProbeFinished;
        event Action<string> NonFatalException;
        event Action<string> Info;
        event Action<string> LineReceived;
        event Action<string> LineSent;
        event Action ConnectionStateChanged;
        event Action PositionUpdateReceived;
        event Action StatusChanged;
        event Action DistanceModeChanged;
        event Action UnitChanged;
        event Action PlaneChanged;
        event Action BufferStateChanged;
        event Action OperatingModeChanged;
        event Action FileChanged;
        event Action FilePositionChanged;

        ReadOnlyCollection<string> File { get; }

        int FilePosition { get; }

        int BufferState { get; }

        Vector3 MachinePosition { get; }
        Vector3 WorkPosition { get; }

        Machine.OperatingMode Mode { get; }

        String Status { get; }

        ParseDistanceMode DistanceMode { get;  }

        ParseUnit Unit { get; }

        ArcPlane Plane { get; }

        bool Connected { get; }

        void Connect(Stream outputStream);

        void Disconnect();

        void SendLine(string line);

        void SoftReset();

        void FeedHold();

        void CycleStart();

        void SetFile(IList<string> file);

        void ClearFile();

        void FileStart();

        void FilePause();

        void ProbeStart();

        void ProbeStop();

        void FileGoto(int lineNumber);

        void ClearQueue();
    }
}
