using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.GCode;
using LagoVista.Core.GCode.Commands;
using LagoVista.GCode.Sender;

namespace LagoVista.GCode.Sender.Application
{
    partial class MainWindow
    {
        private void Machine_PlaneChanged()
        {
            //ButtonArcPlane.Content = App.Current.Machine.Plane.ToString() + "-Plane";
        }

        private void Machine_UnitChanged()
        {
            //ButtonUnit.Content = App.Current.Machine.Unit.ToString();
        }

        private void Machine_DistanceModeChanged()
        {
            //ButtonDistanceMode.Content = App.Current.Machine.DistanceMode.ToString();
        }

        private void Machine_StatusChanged()
        {
            /*ButtonStatus.Content = App.Current.Machine.Status;

            if (App.Current.Machine.Status == "Alarm")
                ButtonStatus.Foreground = Brushes.Red;
            else if (App.Current.Machine.Status == "Hold")
                ButtonStatus.Foreground = Brushes.Yellow;
            else if (App.Current.Machine.Status == "Run")
                ButtonStatus.Foreground = Brushes.Green;
            else
                ButtonStatus.Foreground = Brushes.Black;*/
        }

        private void Machine_PositionUpdateReceived()
        {
            Services.DispatcherServices.Invoke(() =>
            {
                HeightMap.RefreshToolPosition();
            });
        }

        private void Machine_BufferStateChanged()
        {
            //ProgressBarBufferCapacity.Value = App.Current.Machine.BufferState;
            //LabelBufferState.Content = App.Current.Machine.BufferState;
        }

        private void ButtonDistanceMode_Click(object sender, RoutedEventArgs e)
        {
            if (App.Current.Machine.Mode != OperatingMode.Manual)
                return;

            if (App.Current.Machine.DistanceMode == ParseDistanceMode.Absolute)
                App.Current.Machine.SendLine("G91");
            else
                App.Current.Machine.SendLine("G90");
        }

        private void ButtonArcPlane_Click(object sender, RoutedEventArgs e)
        {
            if (App.Current.Machine.Mode != OperatingMode.Manual)
                return;

            if (App.Current.Machine.Plane != ArcPlane.XY)
                App.Current.Machine.SendLine("G17");
        }

        private void ButtonUnit_Click(object sender, RoutedEventArgs e)
        {
            if (App.Current.Machine.Mode != OperatingMode.Manual)
                return;

            if (App.Current.Machine.Unit == ParseUnit.Metric)
                App.Current.Machine.SendLine("G20");
            else
                App.Current.Machine.SendLine("G21");
        }

        private void ButtonStatus_Click(object sender, RoutedEventArgs e)
        {
            if (App.Current.Machine.Mode != OperatingMode.Manual)
                return;

            App.Current.Machine.SendLine("$X");
        }
        

        private void Machine_NonFatalException(string obj)
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = obj;
            item.Foreground = Brushes.Red;
            item.FontSize = 18;

            MachineResponse.AddHistoryItem(item);
        }

        private void Machine_Info(string obj)
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = obj;
            item.Foreground = Brushes.OrangeRed;
            item.FontSize = 14;

            MachineResponse.AddHistoryItem(item);
        }

        private void Machine_LineSent(string obj)
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = obj;
            item.Foreground = Brushes.Black;
            item.FontSize = 14;

            MachineResponse.AddHistoryItem(item);
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

            MachineResponse.AddHistoryItem(item);
        }

        private void Machine_FilePositionChanged()
        {
            CurrentFile.RefreshFileSendStatus();
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
               
                HeightMap.SetPreviewModel(ToolPath.Commands);
            }

            CurrentFile.NewFileLoaded();
        }

        private void UpdateAllButtons()
        {
            HeightMap.ModelToolVisible = App.Current.Machine.Connected;
        }
    }
}
