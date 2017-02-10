using DirectShowLib;
using LagoVista.GCode.Sender.Interfaces;
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

        public SettingsWindow(IMachine machine, int index = 0)
		{
            _machine = machine;
            DataContext = new SettingsViewModel(machine);
            var cameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            var idx = 0;
            foreach (var camera in cameras)
            {
                ViewModel.Cameras.Add(new Models.Camera()
                {
                    Id = camera.DevicePath,
                    Name = camera.Name,
                    CameraIndex = idx++
                });
            }

            InitializeComponent();

            Tabs.SelectedIndex = index;
            
            Closed += SettingsWindow_Closed;
		}
        public SettingsViewModel ViewModel
        {
            get { return DataContext as SettingsViewModel; }
        }

        private async void SettingsWindow_Closed(object sender, EventArgs e)
        {
            await _machine.Settings.SaveAsync();
        }


	}
}
