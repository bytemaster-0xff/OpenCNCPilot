using LagoVista.EaglePCB.Models;
using System;
using System.Collections.Generic;

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
                if (_board != null && HasProject)
                {
                    DrillRack = LagoVista.EaglePCB.Managers.GCodeEngine.GetToolRack(_board, Project);
                }
                else
                {
                    DrillRack = null;
                }

                RaisePropertyChanged(nameof(DrillRack));

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

        public List<DrillRackInfo> DrillRack { get; private set; }

        public bool HasProject
        {
            get { return _project != null; }
        }

        public bool HasTopEtching
        {
            get { return _project != null && !String.IsNullOrEmpty(_project.TopEtchingFilePath); }
        }

        public bool HasBottomEtching
        {
            get { return _project != null && !String.IsNullOrEmpty(_project.BottomEtchingFilePath); }
        }

        public String ProjectFilePath
        {
            get; set;
        }
    }
}
