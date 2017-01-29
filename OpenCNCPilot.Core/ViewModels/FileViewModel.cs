using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public class FileViewModel : ViewModelBase
    {
        IMachine _machine;
        public FileViewModel(IMachine machine)
        {
            _machine = machine;
            OpenGCodeFileCommand = new RelayCommand(OpenGCodeFile);
            OpenHeightMapFileCommand = new RelayCommand(OpenHeightMapFile);
        }

        public async void OpenGCodeFile()
        {
            var fileName = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!String.IsNullOrEmpty(fileName))
            {
                var gcode = await Storage.ReadAllLinesAsync(fileName);
                _machine.SetFile(gcode);
            }
        }

        public void OpenHeightMapFile()
        {

        }

        public RelayCommand OpenGCodeFileCommand { get; private set; }

        public RelayCommand OpenHeightMapFileCommand { get; private set; }
    }
}
