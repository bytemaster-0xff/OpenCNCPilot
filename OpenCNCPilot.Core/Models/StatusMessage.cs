using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Models
{
    public class StatusMessage
    {
        public StatusMessageTypes MessageType { get; private set; }
        public DateTime DateStamp { get; private set; }
        public String Message { get; private set; }        

        public static StatusMessage Create(StatusMessageTypes type, String message)
        {
            return new StatusMessage()
            {
                MessageType = type,
                Message = message,
                DateStamp = DateTime.Now
            };
        }
    }
}
