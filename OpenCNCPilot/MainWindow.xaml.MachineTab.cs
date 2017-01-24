using OpenCNCPilot.Core.Communication;
using System;
using System.Collections.Generic;
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
			if (machine.Mode != Machine.OperatingMode.Disconnected)
				return;

			new SettingsWindow().ShowDialog();
		}

		private void ButtonConnect_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				machine.Connect(null);
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
				machine.Disconnect();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (machine.Connected)
				machine.Disconnect();

            App.Current.Settings.Save(App.Current.StorageService);
		}
	}
}
