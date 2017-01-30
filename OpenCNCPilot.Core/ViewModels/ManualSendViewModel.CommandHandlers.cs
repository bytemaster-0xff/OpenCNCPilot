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
            if(!String.IsNullOrEmpty(ManualCommandText))
            {
                Machine.SendLine(ManualCommandText);
            }
        }

        public void ShowPrevious()
        {
            if (CanShowPrevious())
            {
                ManualCommandText = _commandBuffer[_commandBufferLocation--];
            }
        }

        public void ShowNext()
        {
            if (CanShowNext())
            {
                if (_commandBufferLocation < _commandBuffer.Count - 1)
                    ManualCommandText = _commandBuffer[_commandBufferLocation++];
            }
        }

    }
}
