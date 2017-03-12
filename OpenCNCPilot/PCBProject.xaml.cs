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
    }
}
