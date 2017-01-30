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
            if (AssertNotBusy())
            {
                CurrentJob = new JobProcessor(this, file);
            }
        }

        public void ClearFile()
        {
            if (AssertNotBusy())
            {
                CurrentJob = null;
            }
        }

        public void FileStart()
        {
            if (AssertConnected() && AssertNotBusy())
            {
                if(!HasJob)
                {
                    AddStatusMessage(StatusMessageTypes.Warning, "Please open a job first.");
                    return;
                }

                CurrentJob.Reset();
                CurrentJob.QueueAllItems();

                Mode = OperatingMode.SendingJob;
            }
        }

        public void FilePause()
        {
            if (AssertConnected())
            {

                if (Mode != OperatingMode.SendingJob)
                {
                    AddStatusMessage(StatusMessageTypes.Warning, "Not Sending File");
                    return;
                }

                Mode = OperatingMode.Manual;
            }
        }
    }
}
