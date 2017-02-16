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
        private IJobManager _jobManager;
        public IJobManager JobManager
        {
            get { return _jobManager; }
            set
            {
                _jobManager = value;
                RaisePropertyChanged();
            }
        }

        IHeightMapManager _heightMapManager;
        public IHeightMapManager HeightMapManager
        {
            get { return _heightMapManager; }
            private set
            {
                _heightMapManager = value;
                RaisePropertyChanged();
            }
        }

        IProbingManager _probingManager;
        public IProbingManager ProbingManager
        {
            get { return _probingManager; }
            private set
            {
                _probingManager = value;
                RaisePropertyChanged();
            }
        }

        IBoardManager _boardManager;
        public IBoardManager BoardManager
        {
            get { return _boardManager; }
            private set
            {
                _boardManager = value;
                RaisePropertyChanged();
            }
        }

        IToolChangeManager _toolChangeManager;
        public IToolChangeManager ToolChangeManager
        {
            get { return _toolChangeManager; }
            private set
            {
                _toolChangeManager = value;
                RaisePropertyChanged();
            }
        }

        private bool _isInitialized = false;
        public bool IsInitialized
        {
            get { return _isInitialized; }
            private set
            {
                _isInitialized = value;
                RaisePropertyChanged();
            }
        }

        ObservableCollection<Models.StatusMessage> _messages;
        public ObservableCollection<Models.StatusMessage> Messages
        {
            get { return _messages; }
            private set
            {
                _messages = value;
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
        }

        public Settings Settings { get { return _settings; } }
    }
}
