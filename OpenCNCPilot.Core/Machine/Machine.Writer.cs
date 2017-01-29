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
        public void SendCommand(GCodeCommand cmd)
        {
            var send_line = cmd.Line;
            _writer.Write(send_line);
            _writer.Write('\n');
            _writer.Flush();

            UpdateStatus(send_line);
            AddStatusMessage(StatusMessageTypes.SentLine, send_line);

            BufferState += cmd.MessageLength;

        }
    }
}
