using OpenCNCPilot.Core.Communication;
using OpenCNCPilot.Core.Util;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OpenCNCPilot.Core.GCode;
using OpenCNCPilot.Presentation;
using LagoVista.Core.PlatformSupport;

namespace OpenCNCPilot
{
    partial class MainWindow
    {
        private void Machine_PlaneChanged()
        {
            ButtonArcPlane.Content = App.Current.Machine.Plane.ToString() + "-Plane";
        }

        private void Machine_UnitChanged()
        {
            ButtonUnit.Content = App.Current.Machine.Unit.ToString();
        }

        private void Machine_DistanceModeChanged()
        {
            ButtonDistanceMode.Content = App.Current.Machine.DistanceMode.ToString();
        }

        private void Machine_StatusChanged()
        {
            ButtonStatus.Content = App.Current.Machine.Status;

            if (App.Current.Machine.Status == "Alarm")
                ButtonStatus.Foreground = Brushes.Red;
            else if (App.Current.Machine.Status == "Hold")
                ButtonStatus.Foreground = Brushes.Yellow;
            else if (App.Current.Machine.Status == "Run")
                ButtonStatus.Foreground = Brushes.Green;
            else
                ButtonStatus.Foreground = Brushes.Black;
        }

        private void Machine_PositionUpdateReceived()
        {
            Services.DispatcherServices.Invoke(() =>
            {
                ModelTool.Point1 = (App.Current.Machine.WorkPosition + new Vector3(0, 0, 10)).ToPoint3D().ToMedia3D();
                ModelTool.Point2 = App.Current.Machine.WorkPosition.ToPoint3D().ToMedia3D();

                var nfi = Constants.DecimalOutputFormat;

                LabelPosX.Content = App.Current.Machine.WorkPosition.X.ToString(nfi);
                LabelPosY.Content = App.Current.Machine.WorkPosition.Y.ToString(nfi);
                LabelPosZ.Content = App.Current.Machine.WorkPosition.Z.ToString(nfi);

                LabelPosMX.Content = App.Current.Machine.MachinePosition.X.ToString(nfi);
                LabelPosMY.Content = App.Current.Machine.MachinePosition.Y.ToString(nfi);
                LabelPosMZ.Content = App.Current.Machine.MachinePosition.Z.ToString(nfi);
            });
        }

        private void Machine_BufferStateChanged()
        {
            ProgressBarBufferCapacity.Value = App.Current.Machine.BufferState;
            LabelBufferState.Content = App.Current.Machine.BufferState;
        }

        private void ButtonDistanceMode_Click(object sender, RoutedEventArgs e)
        {
            if (App.Current.Machine.Mode != Machine.OperatingMode.Manual)
                return;

            if (App.Current.Machine.DistanceMode == Core.GCode.ParseDistanceMode.Absolute)
                App.Current.Machine.SendLine("G91");
            else
                App.Current.Machine.SendLine("G90");
        }

        private void ButtonArcPlane_Click(object sender, RoutedEventArgs e)
        {
            if (App.Current.Machine.Mode != Machine.OperatingMode.Manual)
                return;

            if (App.Current.Machine.Plane != Core.GCode.GCodeCommands.ArcPlane.XY)
                App.Current.Machine.SendLine("G17");
        }

        private void ButtonUnit_Click(object sender, RoutedEventArgs e)
        {
            if (App.Current.Machine.Mode != Machine.OperatingMode.Manual)
                return;

            if (App.Current.Machine.Unit == Core.GCode.ParseUnit.Metric)
                App.Current.Machine.SendLine("G20");
            else
                App.Current.Machine.SendLine("G21");
        }

        private void ButtonStatus_Click(object sender, RoutedEventArgs e)
        {
            if (App.Current.Machine.Mode != Machine.OperatingMode.Manual)
                return;

            App.Current.Machine.SendLine("$X");
        }

        private void AddHistoryItem(ListBoxItem item)
        {
            if (ListBoxHistory.Items.Count > 8)
                ListBoxHistory.Items.RemoveAt(0);

            ListBoxHistory.Items.Add(item);
        }

        private void Machine_NonFatalException(string obj)
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = obj;
            item.Foreground = Brushes.Red;
            item.FontSize = 18;

            AddHistoryItem(item);
        }

        private void Machine_Info(string obj)
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = obj;
            item.Foreground = Brushes.OrangeRed;
            item.FontSize = 14;

            AddHistoryItem(item);
        }

        private void Machine_LineSent(string obj)
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = obj;
            item.Foreground = Brushes.Black;
            item.FontSize = 14;

            AddHistoryItem(item);
        }

        private void Machine_LineReceived(string obj)
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = obj;
            item.FontSize = 14;

            if (obj.StartsWith("error"))
                item.Foreground = Brushes.Red;
            else
                item.Foreground = Brushes.Green;

            AddHistoryItem(item);
        }

        private void Machine_FilePositionChanged()
        {
            LabelFilePosition.Content = App.Current.Machine.FilePosition;

            if (ListViewFile.SelectedItem is TextBlock)
                ((TextBlock)ListViewFile.SelectedItem).Background = Brushes.Transparent;

            ListViewFile.SelectedIndex = App.Current.Machine.FilePosition;

            if (ListViewFile.SelectedItem is TextBlock)
                ((TextBlock)ListViewFile.SelectedItem).Background = Brushes.Gray;

            ListViewFile.ScrollIntoView(ListViewFile.SelectedItem);
        }

        private void Machine_FileChanged()
        {
            try
            {
                ToolPath = GCodeFile.FromList(App.Current.Machine.File);
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced); // prevents considerable increase in memory usage
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not parse GCode File, no preview/editing available\nrun this file at your own risk\n" + ex.Message);
            }

            if (App.Current.Settings.EnableCodePreview)
            {
                HeightMap.GetModel(ToolPath.Commands, App.Current.Settings, ModelLine, ModelRapid, ModelArc);
            }

            LabelFileLength.Content = App.Current.Machine.File.Count;

            int digits = (int)Math.Ceiling(Math.Log10(App.Current.Machine.File.Count));

            string format = "D" + digits;

            int i = 1;

            ListViewFile.Items.Clear();
            foreach (string line in App.Current.Machine.File)
            {
                ListViewFile.Items.Add(new TextBlock() { Text = $"{i++.ToString(format)} : {line}" });
            }

        }

        private void UpdateAllButtons()
        {
            ButtonDistanceMode.IsEnabled = App.Current.Machine.Mode == Machine.OperatingMode.Manual;
            ButtonUnit.IsEnabled = App.Current.Machine.Mode == Machine.OperatingMode.Manual;
            ButtonArcPlane.IsEnabled = App.Current.Machine.Mode == Machine.OperatingMode.Manual;
            ButtonStatus.IsEnabled = App.Current.Machine.Mode == Machine.OperatingMode.Manual;

            ButtonFeedHold.IsEnabled = App.Current.Machine.Mode != Machine.OperatingMode.Disconnected;
            ButtonCycleStart.IsEnabled = App.Current.Machine.Mode != Machine.OperatingMode.Disconnected;
            ButtonSoftReset.IsEnabled = App.Current.Machine.Mode == Machine.OperatingMode.Manual;

            ButtonSettings.IsEnabled = App.Current.Machine.Mode == Machine.OperatingMode.Disconnected;

            ButtonFileOpen.IsEnabled = App.Current.Machine.Mode != Machine.OperatingMode.SendFile;
            ButtonFileStart.IsEnabled = App.Current.Machine.Mode == Machine.OperatingMode.Manual;
            ButtonFilePause.IsEnabled = App.Current.Machine.Mode == Machine.OperatingMode.SendFile;
            ButtonFileGoto.IsEnabled = App.Current.Machine.Mode != Machine.OperatingMode.SendFile;
            ButtonFileClear.IsEnabled = App.Current.Machine.Mode != Machine.OperatingMode.SendFile;

            ButtonManualSend.IsEnabled = App.Current.Machine.Mode == Machine.OperatingMode.Manual;
            ButtonManualSetG10Zero.IsEnabled = App.Current.Machine.Mode == Machine.OperatingMode.Manual;
            ButtonManualSetG92Zero.IsEnabled = App.Current.Machine.Mode == Machine.OperatingMode.Manual;
            ButtonManualResetG10.IsEnabled = App.Current.Machine.Mode == Machine.OperatingMode.Manual;

            ButtonEditSimplify.IsEnabled = App.Current.Machine.Mode != Machine.OperatingMode.SendFile;
            ButtonEditArcToLines.IsEnabled = App.Current.Machine.Mode != Machine.OperatingMode.SendFile;
            ButtonEditSplit.IsEnabled = App.Current.Machine.Mode != Machine.OperatingMode.SendFile;

            ModelTool.Visible = App.Current.Machine.Connected;

            UpdateProbeTabButtons();
        }

        private void Machine_ConnectionStateChanged()
        {
            ButtonConnect.Visibility = App.Current.Machine.Connected ? Visibility.Collapsed : Visibility.Visible;
            ButtonDisconnect.Visibility = App.Current.Machine.Connected ? Visibility.Visible : Visibility.Collapsed;

            ButtonSettings.IsEnabled = !App.Current.Machine.Connected;
        }
    }
}
