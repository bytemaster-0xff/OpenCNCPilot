using System;
using System.Windows;
using OpenCNCPilot.Core.Util;
using Microsoft.Win32;
using OpenCNCPilot.Core.GCode;
using OpenCNCPilot.Core.Communication;
using OpenCNCPilot.Presentation;
using OpenCNCPilot.Core;

namespace OpenCNCPilot
{
	public partial class MainWindow : Window
	{        
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


            App.Current.Machine.ConnectionStateChanged += Machine_ConnectionStateChanged;

            ToolPath = GCodeFile.GetEmpty(App.Current.StorageService, App.Current.LoggerService);

            App.Current.Machine.NonFatalException += Machine_NonFatalException;
            App.Current.Machine.Info += Machine_Info;
            App.Current.Machine.LineReceived += Machine_LineReceived;
            App.Current.Machine.LineSent += Machine_LineSent;

            App.Current.Machine.PositionUpdateReceived += Machine_PositionUpdateReceived;
            App.Current.Machine.StatusChanged += Machine_StatusChanged;
            App.Current.Machine.DistanceModeChanged += Machine_DistanceModeChanged;
            App.Current.Machine.UnitChanged += Machine_UnitChanged;
            App.Current.Machine.PlaneChanged += Machine_PlaneChanged;
            App.Current.Machine.BufferStateChanged += Machine_BufferStateChanged;
            App.Current.Machine.OperatingModeChanged += UpdateAllButtons;
            App.Current.Machine.FileChanged += Machine_FileChanged;
            App.Current.Machine.FilePositionChanged += Machine_FilePositionChanged;
            App.Current.Machine.ProbeFinished += Machine_ProbeFinished;

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
						if (App.Current.Machine.Mode == Machine.OperatingMode.Probe || Map != null)
							return;

						OpenHeightMap(file);
					}
					else
					{
						if (App.Current.Machine.Mode == Machine.OperatingMode.SendFile)
							return;

						try
						{
                            App.Current.Machine.SetFile(System.IO.File.ReadAllLines(file));
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
						if (App.Current.Machine.Mode != Machine.OperatingMode.Probe && Map == null)
						{
							e.Effects = DragDropEffects.Copy;
							return;
						}
					}
					else
					{
						if (App.Current.Machine.Mode != Machine.OperatingMode.SendFile)
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
