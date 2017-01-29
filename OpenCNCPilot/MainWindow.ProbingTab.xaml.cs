using System;
using System.Data;
using System.Windows;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender;
using LagoVista.GCode.Sender.Application.Presentation;

namespace LagoVista.GCode.Sender.Application
{
    partial class MainWindow
    {


		NewHeightMapWindow NewHeightMapDialog;

		private void Map_MapUpdated()
		{
            HeightMap.GetModel(Map);			
		}

		private void ButtonHeightmapCreateNew_Click(object sender, RoutedEventArgs e)
		{
			if (App.Current.Machine.Mode == OperatingMode.ProbingHeightMap || Map != null)
				return;

			NewHeightMapDialog = new NewHeightMapWindow();
			NewHeightMapDialog.Owner = this;

			NewHeightMapDialog.Size_Ok += NewHeightMapDialog_Size_Ok;
			NewHeightMapDialog.SelectedSizeChanged += NewHeightMapDialog_SelectedSizeChanged;
			NewHeightMapDialog.Closed += NewHeightMapDialog_Closed;

			NewHeightMapDialog.Show();

			NewHeightMapDialog_SelectedSizeChanged();
		}

		private void NewHeightMapDialog_Closed(object sender, EventArgs e)
		{
			if (NewHeightMapDialog.Ok)
				return;

            HeightMap.Clear();
		}

		private void NewHeightMapDialog_SelectedSizeChanged()
		{
            HeightMap.SetNewModel(NewHeightMapDialog.Min, NewHeightMapDialog.Max, NewHeightMapDialog.GridSize);
        }

		private void NewHeightMapDialog_Size_Ok()
		{
			if (App.Current.Machine.Mode == OperatingMode.ProbingHeightMap || Map != null)
				return;

			Map = new HeightMap(App.Current.Settings, NewHeightMapDialog.GridSize, NewHeightMapDialog.Min, NewHeightMapDialog.Max);
			
			if (NewHeightMapDialog.GenerateTestPattern)
			{
				try
				{
					Map.FillWithTestPattern(NewHeightMapDialog.TestPattern);
					Map.NotProbed.Clear();
				}
				catch { MessageBox.Show("Error in test pattern"); }
			}

			Map.MapUpdated += Map_MapUpdated;
			Map_MapUpdated();
		}

		private void SaveFileDialogHeightMap_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (App.Current.Machine.Mode == OperatingMode.ProbingHeightMap || Map == null)
				return;

			try
			{                
                //TODO: Need to get the commands and save them
				//Map.Save(saveFileDialogHeightMap.FileName);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void OpenFileDialogHeightMap_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			OpenHeightMap(openFileDialogHeightMap.FileName);
		}

		private void OpenHeightMap(string filepath)
		{
			if (App.Current.Machine.Mode == OperatingMode.ProbingHeightMap || Map != null)
				return;

			try
			{
				Map = Presentation.HeightMap.Load(filepath, App.Current.Settings);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			Map.MapUpdated += Map_MapUpdated;
            HeightMap.SetPreviewModel(Map);

			Map_MapUpdated();
		}

		private void ButtonHeightmapLoad_Click(object sender, RoutedEventArgs e)
		{
			if (App.Current.Machine.Mode == OperatingMode.ProbingHeightMap || Map != null)
				return;

			openFileDialogHeightMap.ShowDialog();
		}

		private void ButtonHeightmapSave_Click(object sender, RoutedEventArgs e)
		{
			if (App.Current.Machine.Mode == OperatingMode.ProbingHeightMap || Map == null)
				return;

			saveFileDialogHeightMap.FileName = $"map{(int)Map.Delta.X}x{(int)Map.Delta.Y}.hmap";
			saveFileDialogHeightMap.ShowDialog();
		}

		private void ButtonHeightmapClear_Click(object sender, RoutedEventArgs e)
		{
			if (App.Current.Machine.Mode == OperatingMode.ProbingHeightMap || Map == null)
				return;

			Map = null;

            HeightMap.Clear();
		}

		private void HeightMapProbeNextPoint()
		{
			if (App.Current.Machine.Mode != OperatingMode.ProbingHeightMap)
				return;

			if (!App.Current.Machine.Connected || Map == null || Map.NotProbed.Count == 0)
			{
                App.Current.Machine.ProbeStop();
				return;
			}

			Vector2 nextPoint = Map.GetCoordinates(Map.NotProbed.Peek().Item1, Map.NotProbed.Peek().Item2);

            App.Current.Machine.SendLine($"G0X{nextPoint.X.ToString("0.###", Constants.DecimalOutputFormat)}Y{nextPoint.Y.ToString("0.###", Constants.DecimalOutputFormat)}");

            App.Current.Machine.SendLine($"G38.3Z-{App.Current.Settings.ProbeMaxDepth.ToString("0.###", Constants.DecimalOutputFormat)}F{App.Current.Settings.ProbeFeed.ToString("0.#", Constants.DecimalOutputFormat)}");

            App.Current.Machine.SendLine("G91");
            App.Current.Machine.SendLine($"G0Z{App.Current.Settings.ProbeMinimumHeight.ToString("0.###", Constants.DecimalOutputFormat)}");
            App.Current.Machine.SendLine("G90");
		}

		private void Machine_ProbeFinished(Vector3 position, bool success)
		{
			if (App.Current.Machine.Mode != OperatingMode.ProbingHeightMap)
				return;

			if (!App.Current.Machine.Connected || Map == null || Map.NotProbed.Count == 0)
			{
                App.Current.Machine.ProbeStop();
				return;
			}

			if (!success && App.Current.Settings.AbortOnProbeFail)
			{
				MessageBox.Show("Probe Failed! aborting");

                App.Current.Machine.ProbeStop();
				return;
			}

			Tuple<int, int> lastPoint = Map.NotProbed.Dequeue();

			Map.AddPoint(lastPoint.Item1, lastPoint.Item2, position.Z);

			if (Map.NotProbed.Count == 0)
			{
                App.Current.Machine.SendLine($"G0Z{App.Current.Settings.ProbeSafeHeight.ToString(Constants.DecimalOutputFormat)}");
                App.Current.Machine.ProbeStop();
				return;
			}

			HeightMapProbeNextPoint();
		}

		private void ButtonHeightMapStart_Click(object sender, RoutedEventArgs e)
		{
			if (!App.Current.Machine.Connected || App.Current.Machine.Mode != OperatingMode.Manual || Map == null)
				return;

			if (Map.Progress == Map.TotalPoints)
				return;

			App.Current.Machine.ProbeStart();

			if (App.Current.Machine.Mode != OperatingMode.ProbingHeightMap)
				return;

            App.Current.Machine.SendLine("G90");
            App.Current.Machine.SendLine($"G0Z{App.Current.Settings.ProbeSafeHeight.ToString("0.###", Constants.DecimalOutputFormat)}");

			HeightMapProbeNextPoint();
		}

		private void ButtonHeightMapPause_Click(object sender, RoutedEventArgs e)
		{
			if (App.Current.Machine.Mode != OperatingMode.ProbingHeightMap)
				return;

            App.Current.Machine.ProbeStop();
		}
	}
}
