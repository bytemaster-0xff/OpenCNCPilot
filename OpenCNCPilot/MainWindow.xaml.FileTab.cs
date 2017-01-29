using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LagoVista.GCode.Sender.Application
{
	partial class MainWindow
	{
		private void OpenFileDialogGCode_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (App.Current.Machine.Mode == OperatingMode.SendingJob)
				return;

			try
			{
                App.Current.Machine.SetFile(System.IO.File.ReadAllLines(openFileDialogGCode.FileName));
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void ButtonOpen_Click(object sender, RoutedEventArgs e)
		{
			if (App.Current.Machine.Mode == OperatingMode.SendingJob)
				return;

			openFileDialogGCode.ShowDialog();
		}

		private void ButtonClear_Click(object sender, RoutedEventArgs e)
		{
			if (App.Current.Machine.Mode == OperatingMode.SendingJob)
				return;

            App.Current.Machine.ClearFile();
		}

		private void ButtonFileStart_Click(object sender, RoutedEventArgs e)
		{
            App.Current.Machine.FileStart();
		}

		private void ButtonFilePause_Click(object sender, RoutedEventArgs e)
		{
            App.Current.Machine.FilePause();
		}

		private void ButtonFileGoto_Click(object sender, RoutedEventArgs e)
		{
			if (App.Current.Machine.Mode == OperatingMode.SendingJob)
				return;

			EnterNumberWindow enw = new EnterNumberWindow(App.Current.Machine.FilePosition + 1);
			enw.Title = "Enter new line number";
			enw.Owner = this;
			enw.User_Ok += Enw_User_Ok_Goto;
			enw.Show();
		}

		private void Enw_User_Ok_Goto(double value)
		{
			if (App.Current.Machine.Mode == OperatingMode.SendingJob)
				return;

            App.Current.Machine.FileGoto((int)value - 1);
		}
	}
}
