using System;
using System.Windows;
using Microsoft.Win32;
using LagoVista.Core.GCode;
using LagoVista.GCode.Sender;
using LagoVista.GCode.Sender.Models;
using LagoVista.GCode.Sender.ViewModels;
using LagoVista.GCode.Sender.Application.Presentation;
using LagoVista.GCode.Sender.Util;

namespace LagoVista.GCode.Sender.Application
{
	public partial class MainWindow : Window
	{        
		public MainWindow()
		{
			InitializeComponent();
            this.Loaded += MainWindow_Loaded;
		}

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await App.Current.InitAsync();

            await GrblErrorProvider.InitAsync();

            DataContext = new MainViewModel(App.Current.Machine, App.Current.Settings);            
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
		}
	
        private void SettingsMenu_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().ShowDialog();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(App.Current.Machine.Connected)
            {
                await App.Current.Machine.DisconnectAsync();
            }
        }
    }
}
