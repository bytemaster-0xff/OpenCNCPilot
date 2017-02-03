using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class SettingsViewModel : GCodeAppViewModel
    {
        public SettingsViewModel(IMachine machine) : base(machine)
        {

            InitComamnds();
            Init();
        }

        public override async void Init()
        {
            await InitAsync();
        }

        public override async Task InitAsync()
        {
            if (Machine.Settings.CurrentSerialPort == null)
            {
                Machine.Settings.CurrentSerialPort = new SerialPortInfo()
                {
                    Id = "empty",
                    Name = "-select-"
                };
            }

            var ports = await SLWIOC.Get<IDeviceManager>().GetSerialPortsAsync();
#if DEBUG
            ports.Insert(0, new SerialPortInfo() { Id = "Simulated", Name = "Simulated" });
#endif
            ports.Insert(0, new SerialPortInfo() { Id = "empty", Name = "-select-" });
            SerialPorts = ports;
            RaisePropertyChanged(nameof(SerialPorts));

            var MachineTypes = new List<string>();
            var enums = Enum.GetValues(typeof(FirmwareTypes));
            foreach(var enumType in enums)
            {
                MachineTypes.Add(enumType.ToString().Replace("_","."));
            }
            RaisePropertyChanged(nameof(MachineTypes));

            var gcodeCommands = Enum.GetValues(typeof(JogGCodeCommand));
            GCodeJogCommands = new List<string>();
            foreach (var gcodeCmd in gcodeCommands)
            {
                GCodeJogCommands.Add(gcodeCmd.ToString().Replace("_", "."));
            }

            RaisePropertyChanged(nameof(GCodeJogCommands));
        }
    }
}
