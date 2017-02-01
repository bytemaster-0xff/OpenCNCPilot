using LagoVista.GCode.Sender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MainViewModel
    {
        public JobControlViewModel JobControlVM { get; private set; }
        public MachineControlViewModel MachineControlVM { get; private set; }
        public ManualSendViewModel ManualSendVM { get; private set; }
       
        HeightMap _heightMap;
        public HeightMap HeightMap
        {
            get { return _heightMap; }
            set
            {
                Set(ref _heightMap, value);
                HeightMapProbingManager.HeightMap = value;
                JobControlVM.StartProbeHeightMapCommand.RaiseCanExecuteChanged();
            }
        }

        public Managers.HeightMapProbingManager HeightMapProbingManager { get; private set; }

    }
}
