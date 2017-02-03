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
            ViewModel = new MainViewModel();
            DataContext = ViewModel;
			InitializeComponent();
            this.Loaded += MainWindow_Loaded;
		}

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitAsync();
            await GrblErrorProvider.InitAsync();

        }
	
        private void SettingsMenu_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(ViewModel.Machine).ShowDialog();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await ViewModel.Machine.DisconnectAsync();
        }

        MainViewModel _viewModel;
        public MainViewModel ViewModel
        {
            get { return _viewModel; }
            set { _viewModel = value; }
        }

        private void NewHeigtMap_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            
           var newHeightMapWindow = new NewHeightMapWindow(this, ViewModel.Machine);
           
            if(newHeightMapWindow.ShowDialog().HasValue && newHeightMapWindow.DialogResult.Value)
            {
                ViewModel.HeightMap = newHeightMapWindow.HeightMap;
            }
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            await ViewModel.Machine.Settings.SaveAsync();
        }
    }
}
