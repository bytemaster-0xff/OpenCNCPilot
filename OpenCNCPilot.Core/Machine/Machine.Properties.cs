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
    }
}
