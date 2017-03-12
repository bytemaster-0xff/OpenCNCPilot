using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.EaglePCB.Managers;
using LagoVista.EaglePCB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.GCode.Sender.ViewModels
{
    public class PCBProjectViewModel : ViewModelBase
    {
        PCBProject _project;
        public PCBProject Project
        {
            get { return _project; }
            set { Set(ref _project, value); }
        }

        private PCB _pcb;
        public PCB PCB
        {
            get { return _pcb; }
            set { Set(ref _pcb, value); }
        }

        public PCBProjectViewModel(PCBProject project)
        {
            Project = project;
            OpenBrdCommand = new RelayCommand(OpenBrd);
            SaveDefaultProfileCommand = new RelayCommand(SaveDefaultProfile);
        }

        public async Task LoadDefaultSettings()
        {
            Project = await Storage.GetAsync<PCBProject>("Default.pcbproj");
            if (Project == null)
            {
                Project = PCBProject.Default;
            }
        }

        public async Task<bool> LoadExistingFile(string file)
        {
            Project = await Storage.GetAsync<PCBProject>(file);
            return Project != null;
        }

        public async void SaveDefaultProfile()
        {

            var brdFileName = Project.BoardFile;
            Project.BoardFile = String.Empty;

            await Storage.StoreAsync(Project, "Default.pcbproj");
            Project.BoardFile = brdFileName;
        }

        public async void OpenBrd()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.FileFilterPCB);
            if(!String.IsNullOrEmpty(result))
            {
                try
                {
                    var doc = XDocument.Load(result);
                    PCB = EagleParser.ReadPCB(doc);
                    Project.BoardFile = result;
                }
                catch(Exception)
                {
                    await Popups.ShowAsync("Does not appear to be an Eagle PCB File");
                }
            }
        }

        public RelayCommand OpenBrdCommand { get; private set; }

        public RelayCommand SaveDefaultProfileCommand { get; private set; }
    }
}
