using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class ManualSendViewModel
    {
        private int _commandBufferLocation = 0;
        private List<String> _commandBuffer = new List<string>();

        public void ManualSend()
        {
            _commandBuffer.Add(ManualCommandText);
            _commandBufferLocation = _commandBuffer.Count ;
            if (!String.IsNullOrEmpty(ManualCommandText))
            {
                Machine.SendCommand(ManualCommandText);
                ManualCommandText = String.Empty;
            }
        }

        public void ShowPrevious()
        {
            if (CanShowPrevious())
            {
                --_commandBufferLocation;
                ManualCommandText = _commandBuffer[_commandBufferLocation];
            }
        }

        public void ShowNext()
        {
            if (CanShowNext())
            {
                ++_commandBufferLocation;
                ManualCommandText = _commandBuffer[_commandBufferLocation];
            }
            else
            {
                ++_commandBufferLocation;
                _commandBufferLocation = Math.Min(_commandBufferLocation, _commandBuffer.Count);
                ManualCommandText = string.Empty;
            }
        }
    }
}
