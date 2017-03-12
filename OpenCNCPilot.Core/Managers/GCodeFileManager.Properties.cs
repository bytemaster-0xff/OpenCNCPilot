using LagoVista.Core.GCode;
using LagoVista.Core.GCode.Commands;
using LagoVista.GCode.Sender.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class GCodeFileManager
    {
        public ObservableCollection<Line3D> Lines { get; private set; }

        public ObservableCollection<Line3D> RapidMoves { get; private set; }

        public ObservableCollection<Line3D> Arcs { get; private set; }

        public TimeSpan EstimatedTimeRemaining { get { return _file == null ? TimeSpan.Zero : _file.EstimatedRunTime - ElapsedTime; } }

        public TimeSpan ElapsedTime { get { return _started.HasValue ? DateTime.Now - _started.Value : TimeSpan.Zero; } }

        public DateTime EstimatedCompletion { get { return _started.HasValue ? _started.Value.Add(_file.EstimatedRunTime) : DateTime.Now; } }

        public int CurrentIndex { get { return _head; } }
        public int TotalLines { get { return _file == null ? 0 : _file.Commands.Count; } }

        LagoVista.Core.Models.Drawing.Point3D<double> _min;
        public LagoVista.Core.Models.Drawing.Point3D<double> Min
        {
            get { return _min; }
            set
            {
                _min = value;
                RaisePropertyChanged();
            }
        }

        LagoVista.Core.Models.Drawing.Point3D<double> _max;
        public LagoVista.Core.Models.Drawing.Point3D<double> Max
        {
            get { return _max; }
            set
            {
                _max = value;
                RaisePropertyChanged();
            }
        }

        public int Head
        {
            get { return _head; }
            set { Set(ref _head, value); }
        }

        public int Tail
        {
            get { return _tail; }
            set { Set(ref _tail, value); }
        }

        public GCodeFile File
        {
            get { return _file; }
            set
            {
                _file = value;

                if (value !=null)
                {
                    FindExtents(value);
                    RenderPaths();
                }
                else
                {
                    FileName = "<empty>"; 
                    Max = null;
                    Min = null;
                    ClearPaths();
                }
                
                HasValidFile = _file != null;
                RaisePropertyChanged(nameof(HasValidFile));
                RaisePropertyChanged(nameof(Commands));
                RaisePropertyChanged(nameof(EstimatedTimeRemaining));
                RaisePropertyChanged(nameof(ElapsedTime));
                RaisePropertyChanged(nameof(EstimatedCompletion));
                RaisePropertyChanged(nameof(TotalLines));
                RaisePropertyChanged(nameof(CurrentIndex));

                _head = 0;
                _tail = 0;
            }
        }

        private string _fileName = "<empty>";
        public string FileName
        {
            get { return _fileName; }
            set { Set(ref _fileName, value); }
        }

        public IEnumerable<GCodeCommand> Commands
        {
            get { return _file == null ? null : _file.Commands; }
        }

        private bool _hasValidFile = false;
        public bool HasValidFile
        {
            get { return _hasValidFile; }
            set { Set(ref _hasValidFile, value); }
        }

        public bool IsDirty
        {
            get { return _isDirty; }
            set { Set(ref _isDirty, value); }
        }

        public bool IsCompleted { get { return Tail == TotalLines; } }
    }
}