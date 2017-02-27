using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class SettingsViewModel
    {

        public async void SaveSettingsAsync()
        {
            await Machine.MachineRepo.SaveAsync();
        }

        public void Cancel()
        {

        }
    }
}
