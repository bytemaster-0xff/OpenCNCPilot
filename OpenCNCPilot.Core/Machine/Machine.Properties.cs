using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender
{
    public partial class Machine
    {
        public IJobManager JobManager { get; private set; }

        public IHeightMapManager HeightMapManager { get; private set; }

        public IProbingManager ProbingManager { get;  private set;}

        private bool _isInitialized = false;
        public bool IsInitialized
        {
            get { return _isInitialized; }
            set
            {
                _isInitialized = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Models.StatusMessage> Messages
        {
            get; private set;
        }


        public int MessageCount
        {
            get
            {
                if (Messages == null)
                {
                    return 0;
                }

                return Messages.Count - 1;
            }
            set { }
        }

        public Settings Settings { get { return _settings; } }
    }
}
