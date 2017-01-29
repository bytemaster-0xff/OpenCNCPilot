using LagoVista.GCode.Sender.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LagoVista.GCode.Sender.Application
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		public SettingsWindow()
		{
			InitializeComponent();
            DataContext = new SettingsViewModel(App.Current.Machine, App.Current.Settings);
            Closed += SettingsWindow_Closed;
		}

        private async void SettingsWindow_Closed(object sender, EventArgs e)
        {
            await App.Current.Settings.SaveAsync();
        }
	}
}
