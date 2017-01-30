using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class ManualSendViewModel
    {
        private String _manualCommandText;
        public String ManualCommandText
        {
            get { return _manualCommandText; }
            set { Set(ref _manualCommandText, value); }
        }
    }
}
