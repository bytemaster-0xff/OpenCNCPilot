using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class SettingsViewModel : GCodeAppViewModelBase
    {
        public SettingsViewModel(IMachine machine) : base(machine)
        {
            Cameras = new List<Models.Camera>();
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

            var machineTypes = new ObservableCollection<string>();
            var enums = Enum.GetValues(typeof(FirmwareTypes));
            foreach(var enumType in enums)
            {
                machineTypes.Add(enumType.ToString().Replace("_","."));
            }

            MachineTypes = machineTypes;
            RaisePropertyChanged(nameof(MachineTypes));

            var gcodeCommands = Enum.GetValues(typeof(JogGCodeCommand));
            var gcodeJogCommands = new ObservableCollection<string>();
            foreach (var gcodeCmd in gcodeCommands)
            {
                gcodeJogCommands.Add(gcodeCmd.ToString().Replace("_", "."));
            }

            GCodeJogCommands = gcodeJogCommands;
            RaisePropertyChanged(nameof(GCodeJogCommands));

            var origins = Enum.GetValues(typeof(MachineOrigin));
            var originOptions = new ObservableCollection<string>();
            foreach(var origin in origins)
            {
                originOptions.Add(origin.ToString().Replace("_", " "));
            }

            MachineOrigins = originOptions;
            RaisePropertyChanged(nameof(MachineOrigins));
        }
    }
}
