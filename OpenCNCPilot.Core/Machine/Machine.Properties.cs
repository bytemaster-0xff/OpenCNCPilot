using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender
{
    public partial class Machine
    {
        IJobProcessor _jobProcessor;
        public IJobProcessor CurrentJob
        {
            get { return _jobProcessor; }
            private set
            {
                _jobProcessor = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasJob));
            }
        }

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
