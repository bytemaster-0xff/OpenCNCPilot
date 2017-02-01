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

        IMachine _machine;

        public SettingsWindow(IMachine machine)
		{
            _machine = machine;
            DataContext = new SettingsViewModel(machine);
            InitializeComponent();
            
            Closed += SettingsWindow_Closed;
		}

        private async void SettingsWindow_Closed(object sender, EventArgs e)
        {
            await _machine.Settings.SaveAsync();
        }
	}
}
