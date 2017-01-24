﻿using OpenCNCPilot.Core.Communication;
using OpenCNCPilot.Core.Util;
using OpenCNCPilot.Core.GCode;
using OpenCNCPilot.Presentation;

using System;
using System.Data;
using System.Windows;

namespace OpenCNCPilot
{
    partial class MainWindow
    {
		void UpdateProbeTabButtons()
		{
			ButtonHeightMapCreateNew.IsEnabled = Map == null;
			ButtonHeightMapLoad.IsEnabled = Map == null;
			ButtonHeightMapSave.IsEnabled = machine.Mode != Machine.OperatingMode.Probe && Map != null;
			ButtonHeightMapClear.IsEnabled = machine.Mode != Machine.OperatingMode.Probe && Map != null;

			GridProbingControls.Visibility = Map != null ? Visibility.Visible : Visibility.Collapsed;

			ButtonHeightMapStart.IsEnabled = machine.Mode != Machine.OperatingMode.Probe && Map != null && Map.NotProbed.Count > 0;
			ButtonHeightMapPause.IsEnabled = machine.Mode == Machine.OperatingMode.Probe;

			ButtonEditApplyHeightMap.IsEnabled = machine.Mode != Machine.OperatingMode.SendFile && Map != null && Map.NotProbed.Count == 0;
		}

		NewHeightMapWindow NewHeightMapDialog;

		private void Map_MapUpdated()
		{
			Map.GetModel(ModelHeightMap);
			LabelHeightMapProgress.Content = Map.Progress + "/" + Map.TotalPoints;
		}

		private void ButtonHeightmapCreateNew_Click(object sender, RoutedEventArgs e)
		{
			if (machine.Mode == Machine.OperatingMode.Probe || Map != null)
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

			ModelHeightMapBoundary.Points.Clear();
			ModelHeightMapPoints.Points.Clear();
		}

		private void NewHeightMapDialog_SelectedSizeChanged()
		{
			HeightMap.GetPreviewModel(NewHeightMapDialog.Min, NewHeightMapDialog.Max, NewHeightMapDialog.GridSize, ModelHeightMapBoundary, ModelHeightMapPoints);
		}

		private void NewHeightMapDialog_Size_Ok()
		{
			if (machine.Mode == Machine.OperatingMode.Probe || Map != null)
				return;

			Map = new HeightMap(App.Current.Settings, App.Current.LoggerService, NewHeightMapDialog.GridSize, NewHeightMapDialog.Min, NewHeightMapDialog.Max);
			
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
			UpdateProbeTabButtons();
			Map_MapUpdated();
		}

		private void SaveFileDialogHeightMap_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (machine.Mode == Machine.OperatingMode.Probe || Map == null)
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
			if (machine.Mode == Machine.OperatingMode.Probe || Map != null)
				return;

			try
			{
				Map = HeightMap.Load(filepath, App.Current.Settings, App.Current.LoggerService);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			Map.MapUpdated += Map_MapUpdated;

			Map.GetPreviewModel(ModelHeightMapBoundary, ModelHeightMapPoints);

			UpdateProbeTabButtons();
			Map_MapUpdated();
		}

		private void ButtonHeightmapLoad_Click(object sender, RoutedEventArgs e)
		{
			if (machine.Mode == Machine.OperatingMode.Probe || Map != null)
				return;

			openFileDialogHeightMap.ShowDialog();
		}

		private void ButtonHeightmapSave_Click(object sender, RoutedEventArgs e)
		{
			if (machine.Mode == Machine.OperatingMode.Probe || Map == null)
				return;

			saveFileDialogHeightMap.FileName = $"map{(int)Map.Delta.X}x{(int)Map.Delta.Y}.hmap";
			saveFileDialogHeightMap.ShowDialog();
		}

		private void ButtonHeightmapClear_Click(object sender, RoutedEventArgs e)
		{
			if (machine.Mode == Machine.OperatingMode.Probe || Map == null)
				return;

			Map = null;

			LabelHeightMapProgress.Content = "0/0";

			ModelHeightMap.MeshGeometry = new System.Windows.Media.Media3D.MeshGeometry3D();
			ModelHeightMapBoundary.Points.Clear();
			ModelHeightMapPoints.Points.Clear();

			UpdateProbeTabButtons();
		}

		private void HeightMapProbeNextPoint()
		{
			if (machine.Mode != Machine.OperatingMode.Probe)
				return;

			if (!machine.Connected || Map == null || Map.NotProbed.Count == 0)
			{
				machine.ProbeStop();
				return;
			}

			Vector2 nextPoint = Map.GetCoordinates(Map.NotProbed.Peek().Item1, Map.NotProbed.Peek().Item2);

			machine.SendLine($"G0X{nextPoint.X.ToString("0.###", Constants.DecimalOutputFormat)}Y{nextPoint.Y.ToString("0.###", Constants.DecimalOutputFormat)}");

			machine.SendLine($"G38.3Z-{App.Current.Settings.ProbeMaxDepth.ToString("0.###", Constants.DecimalOutputFormat)}F{App.Current.Settings.ProbeFeed.ToString("0.#", Constants.DecimalOutputFormat)}");

			machine.SendLine("G91");
			machine.SendLine($"G0Z{App.Current.Settings.ProbeMinimumHeight.ToString("0.###", Constants.DecimalOutputFormat)}");
			machine.SendLine("G90");
		}

		private void Machine_ProbeFinished(Vector3 position, bool success)
		{
			if (machine.Mode != Machine.OperatingMode.Probe)
				return;

			if (!machine.Connected || Map == null || Map.NotProbed.Count == 0)
			{
				machine.ProbeStop();
				return;
			}

			if (!success && App.Current.Settings.AbortOnProbeFail)
			{
				MessageBox.Show("Probe Failed! aborting");

				machine.ProbeStop();
				return;
			}

			Tuple<int, int> lastPoint = Map.NotProbed.Dequeue();

			Map.AddPoint(lastPoint.Item1, lastPoint.Item2, position.Z);

			if (Map.NotProbed.Count == 0)
			{
				machine.SendLine($"G0Z{App.Current.Settings.ProbeSafeHeight.ToString(Constants.DecimalOutputFormat)}");
				machine.ProbeStop();
				return;
			}

			HeightMapProbeNextPoint();
		}

		private void ButtonHeightMapStart_Click(object sender, RoutedEventArgs e)
		{
			if (!machine.Connected || machine.Mode != Machine.OperatingMode.Manual || Map == null)
				return;

			if (Map.Progress == Map.TotalPoints)
				return;

			machine.ProbeStart();

			if (machine.Mode != Machine.OperatingMode.Probe)
				return;

			machine.SendLine("G90");
			machine.SendLine($"G0Z{App.Current.Settings.ProbeSafeHeight.ToString("0.###", Constants.DecimalOutputFormat)}");

			HeightMapProbeNextPoint();
		}

		private void ButtonHeightMapPause_Click(object sender, RoutedEventArgs e)
		{
			if (machine.Mode != Machine.OperatingMode.Probe)
				return;

			machine.ProbeStop();
		}
	}
}
