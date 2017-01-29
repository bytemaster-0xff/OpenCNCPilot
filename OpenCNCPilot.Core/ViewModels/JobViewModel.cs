using LagoVista.Core.Commanding;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public class JobViewModel : ViewModelBase
    {
        IMachine _machine;
        ILogger _logger;

        public JobViewModel(IMachine machine, Settings settings, ILogger logger)
        {
            _machine = machine;
            _logger = logger;
        }

        public JobViewModel()
        {
            StartJobCommand = new RelayCommand(StartJob);
        }

        public void StartJob()
        {
            _machine.FileStart();
        }

        public void PauseJob()
        {
            _machine.FilePause();
        }

        public void StopJob()
        {
            _machine.ClearFile();
        }

        public RelayCommand StartJobCommand { get; private set; }
    }
}
