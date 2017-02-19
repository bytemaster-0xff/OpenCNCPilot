using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class JobControlViewModel
    {
        public void EmergencyStop()
        {
            Machine.EmergencyStop();
        }

        public void StopJob()
        {
            
        }

        public void StartProbe()
        {
            Machine.ProbingManager.StartProbe();
        }

        public void StartHeightMap()
        {
            Machine.HeightMapManager.StartProbing();
        }

        public void SendGCodeFile()
        {
            Machine.GCodeFileManager.StartJob();
        }

        public void PauseJob()
        {
            Machine.SetMode(OperatingMode.Manual);
        }

        public void ClearAlarm()
        {
            Machine.ClearAlarm();
        }

        public async void Connect()
        {
            if (Machine.Connected)
            {
                await Machine.DisconnectAsync();
            }
            else
            {
                if (Machine.Settings.CurrentSerialPort.Name == "Simulated")
                {
                    await Machine.ConnectAsync(new SimulatedMachine(Machine.Settings.MachineType));
                }
                else
                {
                    await Machine.ConnectAsync(DeviceManager.CreateSerialPort(Machine.Settings.CurrentSerialPort));
                }
            }
        }

    }
}
