using LagoVista.Core.Models;
using LagoVista.GCode.Sender.Models;
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

        public ObservableCollection<String> MachineTypes { get; private set; }
        public ObservableCollection<String> GCodeJogCommands { get; private set; }
        public ObservableCollection<String> MachineOrigins { get; private set; }

        public String GCodeJogCommand
        {
            get { return Machine.Settings.JogGCodeCommand.ToString(); }
            set
            {
                Machine.Settings.JogGCodeCommand = (JogGCodeCommand)Enum.Parse(typeof(JogGCodeCommand), value);
            }
        }

        public String MachineOrigin
        {
            get { return Machine.Settings.MachineOrigin.ToString().Replace("_", " "); }
            set
            {
                var newValue = value.Replace(" ", "_");
                Machine.Settings.MachineOrigin = (MachineOrigin)Enum.Parse(typeof(MachineOrigin), newValue);
            }
        }

        public List<Camera> Cameras { get; private set; }

        public String PositioningCameraId
        {
            get
            {
                return Machine.Settings.PositioningCamera != null ? Machine.Settings.PositioningCamera.Id : "";
            }
            set
            {
                if(value != null)
                {
                    Machine.Settings.PositioningCamera = Cameras.Where(cmr => cmr.Id == value).FirstOrDefault();
                }
            }
        }

        public String MachineType
        {
            get { return Machine.Settings.MachineType.ToString().Replace("_","."); }
            set { Machine.Settings.MachineType = (FirmwareTypes)Enum.Parse(typeof(FirmwareTypes), value.Replace(".", "_")); }
        }

        public ObservableCollection<SerialPortInfo> SerialPorts { get; private set; }
    }
}
