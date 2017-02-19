using LagoVista.Core.GCode.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public  partial class ToolChangeManager
    {
        public async Task HandleToolChange(MCode mcode)
        {
            await Core.PlatformSupport.Services.Popups.ShowAsync("Tool Change Required\nChange Tool to: " + mcode.DrillSize);
        }
    }
}
