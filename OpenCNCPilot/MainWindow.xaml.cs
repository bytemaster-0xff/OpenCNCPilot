﻿using System;
using System.Windows;
using OpenCNCPilot.Core.Util;
using Microsoft.Win32;
using OpenCNCPilot.Core.GCode;
using OpenCNCPilot.Core.Communication;
using OpenCNCPilot.Presentation;

namespace OpenCNCPilot
{
	public partial class MainWindow : Window
	{
        Machine machine;

		OpenFileDialog openFileDialogGCode = new OpenFileDialog() { InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Filter = Constants.FileFilterGCode };
		OpenFileDialog openFileDialogHeightMap = new OpenFileDialog() { InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Filter = Constants.FileFilterHeightMap };
		SaveFileDialog saveFileDialogHeightMap = new SaveFileDialog() { InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Filter = Constants.FileFilterHeightMap };

        GCodeFile ToolPath { get; set; } 
		HeightMap Map { get; set; }

		public MainWindow()
		{
			InitializeComponent();

			openFileDialogGCode.FileOk += OpenFileDialogGCode_FileOk;
			openFileDialogHeightMap.FileOk += OpenFileDialogHeightMap_FileOk;
			saveFileDialogHeightMap.FileOk += SaveFileDialogHeightMap_FileOk;

			machine.ConnectionStateChanged += Machine_ConnectionStateChanged;

            ToolPath = GCodeFile.GetEmpty(App.Current.StorageService, App.Current.LoggerService);

            machine = new Machine(App.Current.Settings, App.Current.DispatcherService, App.Current.LoggerService);

			machine.NonFatalException += Machine_NonFatalException;
			machine.Info += Machine_Info;
			machine.LineReceived += Machine_LineReceived;
			machine.LineSent += Machine_LineSent;

			machine.PositionUpdateReceived += Machine_PositionUpdateReceived;
			machine.StatusChanged += Machine_StatusChanged;
			machine.DistanceModeChanged += Machine_DistanceModeChanged;
			machine.UnitChanged += Machine_UnitChanged;
			machine.PlaneChanged += Machine_PlaneChanged;
			machine.BufferStateChanged += Machine_BufferStateChanged;
			machine.OperatingModeChanged += UpdateAllButtons;
			machine.FileChanged += Machine_FileChanged;
			machine.FilePositionChanged += Machine_FilePositionChanged;
			machine.ProbeFinished += Machine_ProbeFinished;

			UpdateAllButtons();

		}

		private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
		}

		private void Window_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

				if(files.Length > 0)
				{
					string file = files[0];

					if(file.EndsWith(".hmap"))
					{
						if (machine.Mode == Machine.OperatingMode.Probe || Map != null)
							return;

						OpenHeightMap(file);
					}
					else
					{
						if (machine.Mode == Machine.OperatingMode.SendFile)
							return;

						try
						{
							machine.SetFile(System.IO.File.ReadAllLines(file));
						}
						catch (Exception ex)
						{
							MessageBox.Show(ex.Message);
						}
					}
				}
			}
		}

		private void Window_DragEnter(object sender, DragEventArgs e)
		{
			if(e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

				if (files.Length > 0)
				{
					string file = files[0];

					if (file.EndsWith(".hmap"))
					{
						if (machine.Mode != Machine.OperatingMode.Probe && Map == null)
						{
							e.Effects = DragDropEffects.Copy;
							return;
						}
					}
					else
					{
						if (machine.Mode != Machine.OperatingMode.SendFile)
						{
							e.Effects = DragDropEffects.Copy;
							return;
						}
					}
				}
			}

			e.Effects = DragDropEffects.None;
		}
	}
}
