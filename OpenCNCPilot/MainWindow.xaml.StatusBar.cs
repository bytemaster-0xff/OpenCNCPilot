
using System.Windows;

namespace OpenCNCPilot
{
	partial class MainWindow
	{
		private void ButtonFeedHold_Click(object sender, RoutedEventArgs e)
		{
            App.Current.Machine.FeedHold();
		}

		private void ButtonCycleStart_Click(object sender, RoutedEventArgs e)
		{
            App.Current.Machine.CycleStart();
		}

		private void ButtonSoftReset_Click(object sender, RoutedEventArgs e)
		{
            App.Current.Machine.SoftReset();
		}
	}
}
