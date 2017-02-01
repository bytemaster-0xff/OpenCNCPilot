using LagoVista.Core.GCode;
using LagoVista.Core.GCode.Commands;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender
{
    public interface IMachine : INotifyPropertyChanged
    {
        event Action<Vector3, bool> ProbeFinished;

        int BufferState { get;  }

        Task InitAsync();

        bool IsInitialized { get; set; }

        Vector3 MachinePosition { get; }
        Vector3 WorkPosition { get; }

        OperatingMode Mode { get; }

        String Status { get; }

        bool HasJob { get; }

        int MessageCount { get; }


        ParseDistanceMode DistanceMode { get;  }

        ParseUnit Unit { get; }

        ArcPlane Plane { get; }

        IJobProcessor CurrentJob { get; }


        bool Connected { get; }

        Task ConnectAsync(ISerialPort serialPort);



        Task DisconnectAsync();

        void SendLine(string line);

        void SoftReset();

        void FeedHold();

        void CycleStart();

        void ClearAlarm();


        void SetFile(GCodeFile file);

        void ClearFile();

        void FileStart();

        void FilePause();



        void ProbeStart();

        void ProbeStop();


        void ClearQueue();

        bool BufferSpaceAvailable(int bytes);

        void SendCommand(GCodeCommand cmd);

        void AddStatusMessage(StatusMessageTypes type, String message);

        Settings Settings { get; }
    }
}
