using System;
using System.Data;
using System.Windows;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender;
using LagoVista.GCode.Sender.Application.Presentation;
using LagoVista.GCode.Sender.Models;

namespace LagoVista.GCode.Sender.Application
{
    partial class MainWindow
    {


		NewHeightMapWindow NewHeightMapDialog;
		

		private void ButtonHeightmapCreateNew_Click(object sender, RoutedEventArgs e)
		{
			if (App.Current.Machine.Mode == OperatingMode.ProbingHeightMap )
				return;

			NewHeightMapDialog = new NewHeightMapWindow();
			NewHeightMapDialog.Owner = this;

			NewHeightMapDialog.Size_Ok += NewHeightMapDialog_Size_Ok;
			NewHeightMapDialog.Closed += NewHeightMapDialog_Closed;

			NewHeightMapDialog.Show();
		}

		private void NewHeightMapDialog_Closed(object sender, EventArgs e)
		{
			if (NewHeightMapDialog.Ok)
				return;

            HeightMap.Clear();
		}
        
    
		private void NewHeightMapDialog_Size_Ok()
		{
			if (App.Current.Machine.Mode == OperatingMode.ProbingHeightMap)
				return;

			var map = new HeightMap(App.Current.Settings, NewHeightMapDialog.GridSize, NewHeightMapDialog.Min, NewHeightMapDialog.Max);
			
			if (NewHeightMapDialog.GenerateTestPattern)
			{
				try
				{
                    map.FillWithTestPattern(NewHeightMapDialog.TestPattern);
                    map.NotProbed.Clear();
				}
				catch { MessageBox.Show("Error in test pattern"); }
			}

            HeightMap.HeightMap = map;

		}

	
	}
}
