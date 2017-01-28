using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCNCPilot.Core.Communication
{
    public partial class Machine
    {
        private ReadOnlyCollection<string> _file = new ReadOnlyCollection<string>(new string[0]);
        public ReadOnlyCollection<string> File
        {
            get { return _file; }
            set
            {
                _file = value;
                FilePosition = 0;
                RaiseEvent(FileChanged);
            }
        }

        public void SetFile(IList<string> file)
        {
            if (Mode == OperatingMode.SendFile)
            {
                RaiseEvent(Info, "Can't change file while active");
                return;
            }

            File = new ReadOnlyCollection<string>(file);
            FilePosition = 0;
        }

        public void ClearFile()
        {
            if (Mode == OperatingMode.SendFile)
            {
                RaiseEvent(Info, "Can't change file while active");
                return;
            }

            File = new ReadOnlyCollection<string>(new string[0]);
            FilePosition = 0;
        }

        public void FileStart()
        {
            if (!Connected)
            {
                RaiseEvent(Info, "Not Connected");
                return;
            }

            if (Mode != OperatingMode.Manual)
            {
                RaiseEvent(Info, "Not in Manual Mode");
                return;
            }

            Mode = OperatingMode.SendFile;
        }

        public void FilePause()
        {
            if (!Connected)
            {
                RaiseEvent(Info, "Not Connected");
                return;
            }

            if (Mode != OperatingMode.SendFile)
            {
                RaiseEvent(Info, "Not in SendFile Mode");
                return;
            }

            Mode = OperatingMode.Manual;
        }

        public void FileGoto(int lineNumber)
        {
            if (Mode == OperatingMode.SendFile)
                return;

            if (lineNumber >= File.Count || lineNumber < 0)
            {
                RaiseEvent(NonFatalException, "Line Number outside of file length");
                return;
            }

            FilePosition = lineNumber;
        }

    }
}
