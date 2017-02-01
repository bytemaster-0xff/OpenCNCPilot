using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class SettingsViewModel
    {

        public async void Save()
        {
            await Machine.Settings.SaveAsync();
        }

        public void Cancel()
        {

        }
    }
}
