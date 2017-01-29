using LagoVista.Core.GCode.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender
{
    public partial class Machine
    {
        private String GetNextLine()
        {
            bool eof = false;
            String send_line = null;
            while (send_line == null && !eof)
            {
                if (File.Count > FilePosition &&
                    BufferSpaceAvailable(File[FilePosition].Length + 1))
                {
                    var nextLine = File[FilePosition++];
                    var parts = nextLine.Split(';');
                    if (!String.IsNullOrEmpty(parts[0]))
                    {
                        send_line = parts[0];
                    }
                }
                else
                {
                    eof = true;
                }
            }

            return send_line;
        }

        private void SendFile(StreamWriter writer)
        {
            var send_line = GetNextLine();
            if (!String.IsNullOrEmpty(send_line))
            {
                writer.Write(send_line);
                writer.Write('\n');
                writer.Flush();

                RaiseEvent(UpdateStatus, send_line);
                RaiseEvent(LineSent, send_line);

                BufferState += send_line.Length + 1;

                _sentQueue.Enqueue(send_line);

                if (FilePosition >= File.Count)
                {
                    Mode = OperatingMode.Manual;
                }
            }
        }

        public void SendCommand(GCodeCommand cmd)
        {
            var send_line = cmd.Line;
            _writer.Write(send_line);
            _writer.Write('\n');
            _writer.Flush();

            RaiseEvent(UpdateStatus, send_line);
            RaiseEvent(LineSent, send_line);

            BufferState += cmd.MessageLength;

        }
    }
}
