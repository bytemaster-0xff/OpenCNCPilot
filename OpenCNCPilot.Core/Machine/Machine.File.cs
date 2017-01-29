using LagoVista.Core.GCode;
using LagoVista.GCode.Sender.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace LagoVista.GCode.Sender
{
    public partial class Machine
    {
        public ObservableCollection<StatusMessage> Messages { get; private set; }

        public void SetFile(GCodeFile file)
        {
            if (Mode == OperatingMode.SendingJob)
            {
                AddStatusMessage(StatusMessageTypes.Warning, "Can't change file while active");
                return;
            }

            CurrentJob = new JobProcessor(this, file);
        }

        public void ClearFile()
        {
            if (Mode == OperatingMode.SendingJob)
            {
                AddStatusMessage(StatusMessageTypes.Warning, "Can't change file while active");
                return;
            }

            CurrentJob = null;
        }

        public void FileStart()
        {
            if (!Connected)
            {
                AddStatusMessage(StatusMessageTypes.Warning, "Not Connected");
                return;
            }

            if (Mode != OperatingMode.Manual)
            {
                AddStatusMessage(StatusMessageTypes.Warning, "Busy");
                return;
            }

            Mode = OperatingMode.SendingJob;
        }

        public void FilePause()
        {
            if (!Connected)
            {
                AddStatusMessage(StatusMessageTypes.Warning, "Not Connected");
                return;
            }

            if (Mode != OperatingMode.SendingJob)
            {
                AddStatusMessage(StatusMessageTypes.Warning, "Not Sending File");
                return;
            }

            Mode = OperatingMode.Manual;
        }
    }
}
