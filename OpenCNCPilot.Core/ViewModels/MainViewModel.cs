﻿using System.Threading.Tasks;
using LagoVista.Core.ViewModels;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MainViewModel : GCodeAppViewModel
    {
        public MainViewModel() : base()
        {
            Machine = new Machine();

            InitCommands();
            InitChildViewModels();
        }

        public async override Task InitAsync()
        {
            await Machine.InitAsync();
            //Machine.Settings.PropertyChanged += _settings_PropertyChanged;

            await base.InitAsync();            
        }

        private void InitChildViewModels()
        {
            JobControlVM = new JobControlViewModel(Machine);
            ManualSendVM = new ManualSendViewModel(Machine);
            MachineControlVM = new MachineControlViewModel(Machine);
        }
    }
}
