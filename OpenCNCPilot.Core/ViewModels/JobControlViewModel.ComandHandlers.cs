﻿using System;
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
            Machine.GCodeFileManager.ResetJob();
            Machine.SetMode(OperatingMode.Manual);
        }

        public void FeedHold()
        {
            Machine.FeedHold();
        }

        public void CycleStart()
        {
            Machine.CycleStart();
        }

        public void SoftReset()
        {
            Machine.SoftReset();
            if(Machine.GCodeFileManager.HasValidFile)
            {
                Machine.GCodeFileManager.ResetJob();
            }
        }

        public void SpindleOn()
        {
            Machine.SpindleOn();
        }

        public void SpindleOff()
        {
            Machine.SpindleOff();
        }

        public void LaserOn()
        {
            Machine.LaserOn();
        }

        public void LaserOff()
        {
            Machine.LaserOff();
        }

        public void GotoWorkspaceHome()
        {
            Machine.GotoWorkspaceHome();
        }

        public void SetFavorite1()
        {
            Machine.SetFavorite1();
        }

        public void SetFavorite2()
        {
            Machine.SetFavorite2();
        }

        public void GotoFavorite1()
        {
            Machine.GotoFavorite1();
        }

        public void GotoFavorite2()
        {
            Machine.GotoFavorite2();
        }

        public void HomingCycle()
        {
            Machine.HomingCycle();
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
