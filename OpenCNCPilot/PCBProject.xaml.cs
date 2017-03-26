using LagoVista.EaglePCB.Models;
using LagoVista.GCode.Sender.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LagoVista.GCode.Sender.Application
{
    /// <summary>
    /// Interaction logic for PCBProject.xaml
    /// </summary>
    public partial class PCBProject : Window
    {
        public PCBProject()
        {
            InitializeComponent();
            var designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!designTime)
            {
                this.Loaded += PCBProject_Loaded;
            }
        }

        private void PCBProject_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.GenerateIsolationEvent += (s, a) => PCB.PCB2Gode.CreateGCode(ViewModel.Project.EagleBRDFilePath, ViewModel.Project);
        }

        public bool IsNew
        {
            get; set;
        }

        public string PCBFilepath { get; set; }

        public PCBProjectViewModel ViewModel { get { return DataContext as PCBProjectViewModel; } }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsNew)
            {
                var NewFilePath = await Core.PlatformSupport.Services.Popups.ShowSaveFileAsync(String.Empty, Constants.PCBProject);
                if (!String.IsNullOrEmpty(NewFilePath))
                {
                    PCBFilepath = NewFilePath;
                    await ViewModel.Project.SaveAsync(NewFilePath);
                    DialogResult = true;
                }
            }
            else
            {
                await ViewModel.Project.SaveAsync(PCBFilepath);
                DialogResult = true;
            }
        }

        public string NewFilePath { get; set; }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var selectedDrill = (sender as Button).DataContext as Hole;

            ViewModel.Project.Fiducials.Add(selectedDrill);
        }

        private void ConsolidatedDrills_Drop(object sender, DragEventArgs e)
        {
            if (ViewModel.ConsolidatedDrillBit != null)
            {
                var drill = e.Data.GetData("Drill") as DrillBit;
                var existingDrill = ViewModel.AddDrillBit(drill);
                if (!String.IsNullOrEmpty(existingDrill))
                {
                    MessageBox.Show($"This drill bit already exists on: {existingDrill}");
                }
            }
        }

        private void SourceBitsGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                var drill = (sender as Grid).DataContext;
                var data = new DataObject();
                data.SetData("Drill", drill);
                DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        private void ConsolidatedBitsGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var drill = (sender as Grid).DataContext;

                var data = new DataObject();
                data.SetData("Drill", drill);
                DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }


        private void AddConsolidatedDrill_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ConsolidatedDrillBit();
            dlg.ShowForNew(ViewModel.Project, this);
            if (dlg.DialogResult.HasValue && dlg.DialogResult.Value)
            {
                ViewModel.ConsolidatedDrillBit = dlg.ConsolidatedDrill;
            }
        }

        private void ConsolidatedDrillBitItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount >= 2)
            {
                var consolidatedDrill = (sender as TextBlock).DataContext as LagoVista.EaglePCB.Models.ConsolidatedDrillBit;
                var dlg = new ConsolidatedDrillBit();
                dlg.ShowForEdit(ViewModel.Project, consolidatedDrill, this);
            }
        }

        private void FullDrillIst_Drop(object sender, DragEventArgs e)
        {
            if (ViewModel.ConsolidatedDrillBit != null)
            {
                var drill = e.Data.GetData("Drill") as DrillBit;
                ViewModel.RemoveBit(drill);                
            }
        }
    }
}