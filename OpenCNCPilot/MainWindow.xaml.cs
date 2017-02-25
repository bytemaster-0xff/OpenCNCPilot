using System;
using System.Windows;
using Microsoft.Win32;
using LagoVista.Core.GCode;
using LagoVista.GCode.Sender;
using LagoVista.GCode.Sender.Models;
using LagoVista.GCode.Sender.ViewModels;
using LagoVista.GCode.Sender.Util;

namespace LagoVista.GCode.Sender.Application
{
	public partial class MainWindow : Window
	{        
		public MainWindow()
		{
            bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(
                new DependencyObject());
            if (!designTime)
            {
                ViewModel = new MainViewModel();
                DataContext = ViewModel;
                InitializeComponent();
                this.Loaded += MainWindow_Loaded;
            }
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
            ImageSensor.ShutDown();
            if (ViewModel.Machine.Connected)
            {
                await ViewModel.Machine.DisconnectAsync();
            }
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
            
           var newHeightMapWindow = new NewHeightMapWindow(this, ViewModel.Machine, false);
           
            if(newHeightMapWindow.ShowDialog().HasValue && newHeightMapWindow.DialogResult.Value)
            {
                ViewModel.Machine.HeightMapManager.NewHeightMap(newHeightMapWindow.HeightMap);
            }
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            await ViewModel.Machine.Settings.SaveAsync();
        }

        private void NewGeneratedHeigtMap_Click(object sender, RoutedEventArgs e)
        {
            var heightMap = new HeightMap(ViewModel.Machine, ViewModel.Logger);
            heightMap.FillWithTestPattern();
            ViewModel.Machine.HeightMapManager.NewHeightMap(heightMap);
        }

        private void EditHeightMap_Click(object sender, RoutedEventArgs e)
        {
            var newHeightMapWindow = new NewHeightMapWindow(this, ViewModel.Machine, true);
            newHeightMapWindow.ShowDialog();
        }

        private void CloseMenu_Click(object sender, RoutedEventArgs e)
        {        
            Close();
        }

        private void TextBoxManual_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Up)
            {
                ViewModel.ManualSendVM.ShowPrevious();
            }
            else if(e.Key == System.Windows.Input.Key.Down)
            {
                ViewModel.ManualSendVM.ShowNext();
            }
            else if(e.Key == System.Windows.Input.Key.Return)
            {
                e.Handled = true;
                ViewModel.ManualSendVM.ManualSend();
            }
        }

        private void TextBoxManual_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TextBoxManual.CaretIndex == 0)
            {
                TextBoxManual.CaretIndex = TextBoxManual.Text.Length;
            }
        }
    }
}
