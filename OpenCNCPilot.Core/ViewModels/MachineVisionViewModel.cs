using LagoVista.Core.Commanding;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MachineVisionViewModel : GCodeAppViewModelBase
    {
        public MachineVisionViewModel(IMachine machine) : base(machine)
        {
            Profile = new Models.VisionProfile();
            SaveProfileCommand = new RelayCommand(SaveProfile);
        }

        public override async Task InitAsync()
        {
            var profile = await Storage.GetAsync<Models.VisionProfile>("Vision.json");
            if(profile != null)
            {
                Profile = profile;
            }            
        }

        public async void SaveProfile()
        {
            await Storage.StoreAsync(Profile, "Vision.json");
        }

        public RelayCommand SaveProfileCommand { get; private set; }
    }
}
