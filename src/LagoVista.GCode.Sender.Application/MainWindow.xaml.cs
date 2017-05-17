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
using System.Threading.Tasks;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.ViewModels;
using LagoVista.PickAndPlace.Repos;

namespace LagoVista.GCode.Sender.Application
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            _this = this;
            var designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
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

        /* Make the main Window Available to contorls */
        static MainWindow _this;
        public static MainWindow This { get { return _this; } }

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
            if (ViewModel.Machine.Connected)
            {
                MessageBox.Show("Please disconnect before switching machines.");
            }
            else
            {
                ViewModel.Machine.Settings = ViewModel.Machine.MachineRepo.Machines.Where(mach => mach.Id == (sender as MenuItem).Tag.ToString()).FirstOrDefault();
                foreach (var item in MachinesMenu.Items)
                {
                    var menuItem = item as MenuItem;
                    if (menuItem != null)
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
            await ViewModel.LoadMRUs();

            foreach (var file in ViewModel.MRUs.BoardFiles)
            {
                var boardMenu = new MenuItem() { Header = file, Tag = file };
                boardMenu.Click += BoardMenu_Click;
                RecentBoards.Items.Add(boardMenu);
            }

            foreach (var file in ViewModel.MRUs.ProjectFiles)
            {
                var projectMenu = new MenuItem() { Header = file, Tag = file };
                projectMenu.Click += ProjectMenu_Click;
                RecentProjects.Items.Add(projectMenu);
            }

            foreach (var file in ViewModel.MRUs.GCodeFiles)
            {
                var gcodeFile = new MenuItem() { Header = file, Tag = file };
                gcodeFile.Click += GcodeFile_Click;
                RecentGCodeFiles.Items.Add(gcodeFile);
            }
        }

        private async void GcodeFile_Click(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            await ViewModel.Machine.GCodeFileManager.OpenFileAsync(menu.Tag as string);
        }

        private async void ProjectMenu_Click(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            await ViewModel.OpenProjectAsync(menu.Tag as String);
        }

        private async void BoardMenu_Click(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            await ViewModel.Machine.PCBManager.OpenFileAsync(menu.Tag as String);
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

            if (ViewModel.Machine.PCBManager.HasProject && ViewModel.Machine.PCBManager.HasBoard)
            {
                var heightMap = new HeightMap(ViewModel.Machine, ViewModel.Logger);
                heightMap.Min = new Core.Models.Drawing.Vector2(ViewModel.Machine.PCBManager.Project.ScrapSides, ViewModel.Machine.PCBManager.Project.ScrapTopBottom);
                heightMap.Max = new Core.Models.Drawing.Vector2(ViewModel.Machine.PCBManager.Board.Width + ViewModel.Machine.PCBManager.Project.ScrapSides, ViewModel.Machine.PCBManager.Board.Height + ViewModel.Machine.PCBManager.Project.ScrapTopBottom);
                heightMap.GridSize = ViewModel.Machine.PCBManager.Project.HeightMapGridSize;
                ViewModel.Machine.HeightMapManager.NewHeightMap(heightMap);
            }
            else
            {
                var newHeightMapWindow = new NewHeightMapWindow(this, ViewModel.Machine, false);

                if (newHeightMapWindow.ShowDialog().HasValue && newHeightMapWindow.DialogResult.Value)
                {
                    ViewModel.Machine.HeightMapManager.NewHeightMap(newHeightMapWindow.HeightMap);
                }
            }
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            await ViewModel.Machine.MachineRepo.SaveAsync();
        }

        private void NewGeneratedHeigtMap_Click(object sender, RoutedEventArgs e)
        {
            var heightMap = new HeightMap(ViewModel.Machine, ViewModel.Logger);
            ViewModel.Machine.HeightMapManager.CreateTestPattern();
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

        private async void EditMachineMenu_Click(object sender, RoutedEventArgs e)
        {
            //Clone in case we cancel.
            var clonedSettings = ViewModel.Machine.Settings.Clone();
            var dlg = new SettingsWindow(ViewModel.Machine, clonedSettings);
            dlg.Owner = this;
            dlg.ShowDialog();
            if (dlg.DialogResult.HasValue && dlg.DialogResult.Value)
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

        private void PCB2GCode_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Project != null && !String.IsNullOrEmpty(ViewModel.Project.EagleBRDFilePath))
            {
                PCB.PCB2Gode.CreateGCode(ViewModel.Project.EagleBRDFilePath, ViewModel.Project);
            }
            else
            {
                MessageBox.Show("Please Create or Edit a Project PCB->New Project and Assign an Eagle Board File.");
            }
        }

        private async void OpenPCBProject_Click(object sender, RoutedEventArgs e)
        {
            var file = await Core.PlatformSupport.Services.Popups.ShowOpenFileAsync(Constants.PCBProject);
            if (!String.IsNullOrEmpty(file))
            {
                await ViewModel.OpenProjectAsync(file);
            }
        }

        private void ClosePCBProject_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Project = null;
        }

        private void EditPCBProject_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Project == null)
            {
                MessageBox.Show("Please Open or Create a Project First.");
                return;
            }
            var clonedProject = ViewModel.Project.Clone();
            var vm = new PCBProjectViewModel(clonedProject);

            var pcbWindow = new PCBProject();
            pcbWindow.DataContext = vm;
            pcbWindow.IsNew = false;
            pcbWindow.Owner = this;
            pcbWindow.PCBFilepath = ViewModel.Machine.PCBManager.ProjectFilePath;
            pcbWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            pcbWindow.ShowDialog();
            if (pcbWindow.DialogResult.HasValue && pcbWindow.DialogResult.Value)
            {
                ViewModel.Project = vm.Project;
            }
        }

        private async void NewPCBProject_Click(object sender, RoutedEventArgs e)
        {
            var pcbWindow = new PCBProject();
            var vm = new PCBProjectViewModel(new EaglePCB.Models.PCBProject());
            await vm.LoadDefaultSettings();
            pcbWindow.DataContext = vm;
            pcbWindow.IsNew = true;
            pcbWindow.Owner = this;
            pcbWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            pcbWindow.ShowDialog();
            if (pcbWindow.DialogResult.HasValue && pcbWindow.DialogResult.Value)
            {
                ViewModel.Project = vm.Project;
                ViewModel.AddProjectFileMRU(pcbWindow.PCBFilepath);
                if (!String.IsNullOrEmpty(vm.Project.EagleBRDFilePath))
                {
                    await ViewModel.Machine.PCBManager.OpenFileAsync(vm.Project.EagleBRDFilePath);
                }
                ViewModel.Machine.PCBManager.Project = vm.Project;
            }
        }
        

        private void EditPackageLibrary_Click(object sender, RoutedEventArgs e)
        {
            var librWindow = new Views.PackageLibraryWindow();
            librWindow.Owner = this;
            librWindow.ShowDialog();
        }

        private void EditFeederLibrary_Click(object sender, RoutedEventArgs e)
        {
            var lbrWindow = new Views.FeederLibraryWindow();
            lbrWindow.Owner = this;
            lbrWindow.ShowDialog();
        }

        private async void OpenPnPJob_Click(object sender, RoutedEventArgs e)
        {
            var file = await Core.PlatformSupport.Services.Popups.ShowOpenFileAsync(Constants.PickAndPlaceProject);
            if (!String.IsNullOrEmpty(file))
            {
                var job = await Core.PlatformSupport.Services.Storage.GetAsync<PnPJob>(file);
                await job.OpenAsync();
                var pnpViewModel = new PnPJobViewModel(job);
                pnpViewModel.FileName = file;
                await pnpViewModel.InitAsync();
                var jobWindow = new Views.PNPJobWindow();
                jobWindow.DataContext = pnpViewModel;
                jobWindow.Owner = this;
                jobWindow.ShowDialog();
            }
        }

        private async void NewPnPJob_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Machine.PCBManager.HasProject)
            {
                if (ViewModel.Machine.PCBManager.HasBoard)
                {
                    var job = new PnPJob();
                    job.Board = ViewModel.Machine.PCBManager.Board;
                    job.EagleBRDFilePath = ViewModel.Machine.PCBManager.Project.EagleBRDFilePath;
                    var pnpViewModel = new PnPJobViewModel(job);
                    await pnpViewModel.InitAsync();
                    var jobWindow = new Views.PNPJobWindow();
                    jobWindow.DataContext = pnpViewModel;
                    jobWindow.Owner = this;
                    jobWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Please make sure your PCB Project includes a PCB.");
                }
            }
            else
            {
                MessageBox.Show("Please open or create a PCB Project First.");
            }
        }
    }
}
