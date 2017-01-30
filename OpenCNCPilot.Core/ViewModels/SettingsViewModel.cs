﻿using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public Settings Settings { get; private set; }
        private IMachine _machine;
        public SettingsViewModel(IMachine machine, Settings settings)
        {
            Settings = settings;
            _machine = machine;
            if(settings.CurrentSerialPort == null)
            {
                settings.CurrentSerialPort = new SerialPortInfo()
                {
                    Id = "empty",
                    Name = "-select-"
                };
            }
            Init();

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }



        public String SelectedPortId
        {
            get { return Settings.CurrentSerialPort.Id; }
            set
            {
                var port = SerialPorts.Where(prt => prt.Id == value).FirstOrDefault();
                if (port == null)
                    port = SerialPorts.First();
                else
                {
                    port.BaudRate = 115200;
                }

                Settings.CurrentSerialPort = port;
            }
        }

        public override async void Init()
        {
            await InitAsync();
        }

        public override async Task InitAsync()
        {
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

        public bool CanChangeMachineConfig
        {
            get { return !_machine.Connected; }
        }


        public void Save()
        {

        }

        public void Cancel()
        {

        }

        public List<String> MachineTypes { get; private set; }

        public String MachineType
        {
            get { return Settings.MachineType.ToString(); }
            set { Settings.MachineType = (FirmwareTypes)Enum.Parse(typeof(FirmwareTypes), value.Replace(".","_")); }
        }

        public ObservableCollection<SerialPortInfo> SerialPorts { get; private set; }

        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }


    }
}