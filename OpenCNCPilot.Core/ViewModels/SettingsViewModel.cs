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
        public SettingsViewModel(IMachine machine, Settings settings) : base(machine, settings)
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
            if (Settings.CurrentSerialPort == null)
            {
                Settings.CurrentSerialPort = new SerialPortInfo()
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

            var machineTypes = new List<string>();
            var enums = Enum.GetValues(typeof(FirmwareTypes));
            foreach(var enumType in enums)
            {
                machineTypes.Add(enumType.ToString().Replace("_","."));
            }

            MachineTypes = machineTypes;
            RaisePropertyChanged(MachineType);
        }
    }
}
