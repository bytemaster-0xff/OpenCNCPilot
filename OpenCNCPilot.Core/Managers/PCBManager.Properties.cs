using LagoVista.EaglePCB.Models;
using System;


namespace LagoVista.GCode.Sender.Managers
{
    public partial class PCBManager
    {
        private PCB _board;
        public PCB Board
        {
            get { return _board; }
            set
            {
                _board = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasBoard));
            }
        }

        public bool HasBoard
        {
            get { return _board != null; }
        }


        PCBProject _project;
        public PCBProject Project
        {
            get { return _project; }
            set
            {
                _project = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasProject));
                if (_project != null)
                {
                    if (_project != null && !String.IsNullOrEmpty(_project.EagleBRDFilePath))
                    {
                        OpenFileAsync(_project.EagleBRDFilePath);
                    }
                }
                else
                {
                    Board = null;
                }
            }
        }

        public bool HasProject
        {
            get { return _project != null; }
        }

        public String ProjectFilePath
        {
            get; set;
        }
    }
}
