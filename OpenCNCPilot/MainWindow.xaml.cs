using System;
using System.Windows;
using Microsoft.Win32;
using LagoVista.Core.GCode;
using LagoVista.GCode.Sender;
using System.Linq;
using LagoVista.GCode.Sender.Models;
using LagoVista.GCode.Sender.ViewModels;
using LagoVista.GCode.Sender.Util;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;

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
                var repo = LoadRepo();
                ViewModel = new MainViewModel(repo);
                DataContext = ViewModel;
                InitializeComponent();

                foreach (var machine in repo.Machines)
                {
                    var menu = new MenuItem() { Header = machine.MachineName };
                    menu.Tag = machine.Id;
                    if (repo.CurrentMachineId == machine.Id)
                    {
                        menu.IsChecked = true;
                        ViewModel.Machine.Settings = machine;
                    }
                    menu.Click += ChangeMachine_Click;

                    MachinesMenu.Items.Add(menu);
                }
                this.Loaded += MainWindow_Loaded;
            }
        }

        /* Doing this long syncronously here so the data will be ready before creating 
         * the ViewModel and creating the UI 
         * Need to investigate calling InitializeCompoent AFTER some async calls */
        private MachinesRepo LoadRepo()
        {
            MachinesRepo repo;

            var name = Process.GetCurrentProcess().ProcessName;

            /* The XPlat stuff will store the repo in the AppData folder so expect it there */
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            dir = Path.Combine(dir, name);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var jsonPath = Path.Combine(dir, MachinesRepo.FileName);

            if (System.IO.File.Exists(jsonPath))
            {
                var json = System.IO.File.ReadAllText(jsonPath);

                try
                {
                    repo = JsonConvert.DeserializeObject<MachinesRepo>(json);
                }
                catch (Exception)
                {
                    repo = MachinesRepo.Default;
                    System.IO.File.WriteAllText(jsonPath, JsonConvert.SerializeObject(repo));
                }
            }
            else
            {
                repo = MachinesRepo.Default;
                System.IO.File.WriteAllText(jsonPath, JsonConvert.SerializeObject(repo));
            }

            return repo;
        }

        private void ChangeMachine_Click(object sender, RoutedEventArgs e)
        {
            if(ViewModel.Machine.Connected)
            {
                MessageBox.Show("Please disconnect before switching machines.");
            }
            else
            {
                ViewModel.Machine.Settings = ViewModel.Machine.MachineRepo.Machines.Where(mach => mach.Id == (sender as MenuItem).Tag.ToString()).FirstOrDefault();
                foreach (var item in MachinesMenu.Items)
                {
                    var menuItem = item as MenuItem;
                    if(menuItem != null)
                    {
                        menuItem.IsChecked = (string)menuItem.Tag == ViewModel.Machine.MachineRepo.CurrentMachineId;

                    }
                }
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            await ViewModel.InitAsync();
            await GrblErrorProvider.InitAsync();
        }

        private void SettingsMenu_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(ViewModel.Machine, ViewModel.Machine.Settings).ShowDialog();
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

            if (newHeightMapWindow.ShowDialog().HasValue && newHeightMapWindow.DialogResult.Value)
            {
                ViewModel.Machine.HeightMapManager.NewHeightMap(newHeightMapWindow.HeightMap);
            }
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            await ViewModel.Machine.MachineRepo.SaveAsync();
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
            else if (e.Key == System.Windows.Input.Key.Down)
            {
                ViewModel.ManualSendVM.ShowNext();
            }
            else if (e.Key == System.Windows.Input.Key.Return)
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

        private async void EditMachineMenu_Click(object sender, RoutedEventArgs e)
        {
            //Clone in case we cancel.
            var clonedSettings = ViewModel.Machine.Settings.Clone();
            var dlg = new SettingsWindow(ViewModel.Machine, clonedSettings);
            dlg.Owner = this;
            dlg.ShowDialog();
            if(dlg.DialogResult.HasValue && dlg.DialogResult.Value)
            {
                ViewModel.Machine.MachineRepo.Machines.Remove(ViewModel.Machine.Settings);
                ViewModel.Machine.MachineRepo.Machines.Add(clonedSettings);
                ViewModel.Machine.Settings = clonedSettings;
                await ViewModel.Machine.MachineRepo.SaveAsync();
            }
        }

        private async void NewMachinePRofile_Click(object sender, RoutedEventArgs e)
        {
            var settings = MachineSettings.Default;
            settings.MachineName = String.Empty;

            var dlg = new SettingsWindow(ViewModel.Machine, settings);
            dlg.Owner = this;
            dlg.ShowDialog();
            if (dlg.DialogResult.HasValue && dlg.DialogResult.Value)
            {
                ViewModel.Machine.MachineRepo.Machines.Add(settings);
                await ViewModel.Machine.MachineRepo.SaveAsync();

                var menu = new MenuItem() { Header = settings.MachineName };
                menu.Tag = settings.Id;
                menu.Click += ChangeMachine_Click;

                MachinesMenu.Items.Add(menu);

            }
        }
    }
}
