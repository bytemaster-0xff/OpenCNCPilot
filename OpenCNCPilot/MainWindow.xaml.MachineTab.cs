﻿using OpenCNCPilot.Core.Communication;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenCNCPilot
{
	partial class MainWindow
	{
		private void ButtonSettings_Click(object sender, RoutedEventArgs e)
		{
			if (App.Current.Machine.Mode != Machine.OperatingMode.Disconnected)
				return;

			new SettingsWindow().ShowDialog();
		}

		private void ButtonConnect_Click(object sender, RoutedEventArgs e)
		{
			try
			{
                if (App.Current.Settings.CurrentSerialPort.Name == "Simulated")
                {
                    App.Current.Machine.Connect(new SimulatedGCodeMachine());
                }
                else
                {
                    var serialPort = new SerialPort(App.Current.Settings.CurrentSerialPort.Id, App.Current.Settings.CurrentSerialPort.BaudRate);
                    serialPort.Open();
                    App.Current.Machine.Connect(serialPort.BaseStream);
                }
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void ButtonDisconnect_Click(object sender, RoutedEventArgs e)
		{
			try
			{
                App.Current.Machine.Disconnect();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private  async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (App.Current.Machine.Connected)
                App.Current.Machine.Disconnect();

            await App.Current.Settings.SaveAsync();
		}
	}
}
