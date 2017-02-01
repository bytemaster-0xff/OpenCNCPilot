using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class SettingsViewModel
    {

        public String SelectedPortId
        {
            get { return Machine.Settings.CurrentSerialPort.Id; }
            set
            {
                var port = SerialPorts.Where(prt => prt.Id == value).FirstOrDefault();
                if (port == null)
                    port = SerialPorts.First();
                else
                {
                    port.BaudRate = 115200;
                }

                Machine.Settings.CurrentSerialPort = port;
            }
        }

        public List<String> MachineTypes { get; private set; }

        public String MachineType
        {
            get { return Machine.Settings.MachineType.ToString().Replace("_","."); }
            set { Machine.Settings.MachineType = (FirmwareTypes)Enum.Parse(typeof(FirmwareTypes), value.Replace(".", "_")); }
        }

        public ObservableCollection<SerialPortInfo> SerialPorts { get; private set; }
    }
}
