using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class JobControlViewModel
    {
        public void Kill()
        {
         
        }

        public void StopJob()
        {

        }


        public void StartProbeJob()
        {

        }

        public void StartProbeHeight()
        {

        }

        public void StartJob()
        {
            Machine.FileStart();
        }

        public void PauseJob()
        {
            switch (Machine.Mode)
            {
                case OperatingMode.SendingJob: Machine.FilePause(); break;
            }
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
