using LagoVista.Core.Models.Drawing;
using LagoVista.EaglePCB.Models;
using LagoVista.GCode.Sender.Interfaces;
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

        Point2D<double> _firstFiducial;
        public Point2D<double> FirstFiducial
        {
            get { return _firstFiducial; }
            set { Set(ref _firstFiducial, value); }
        }

        Point2D<double> _secondFiducial;
        public Point2D<double> SecondFiducial
        {
            get { return _secondFiducial; }
            set { Set(ref _secondFiducial, value); }
        }

        private Point2D<double> _measuredOffset;
        public Point2D<double> MeasuredOffset
        {
            get { return _measuredOffset; }        
        }

        private double _measuredOffsetAngle;
        public double MeasuredOffsetAngle
        {
            get { return _measuredOffsetAngle; }
        }

        public void SetMeasuredOffset(Point2D<double> offset, double angleDegrees)
        {
            _measuredOffset = offset;
            _measuredOffsetAngle = angleDegrees;
            RaisePropertyChanged(nameof(HasMeasuredOffset));
            RaisePropertyChanged(nameof(MeasuredOffsetAngle));
            RaisePropertyChanged(nameof(MeasuredOffset));
        }

        public void ClearMeasuredOffset()
        {
            _measuredOffset = null;
            _measuredOffsetAngle = 0;

            RaisePropertyChanged(nameof(HasMeasuredOffset));
            RaisePropertyChanged(nameof(MeasuredOffsetAngle));
            RaisePropertyChanged(nameof(MeasuredOffset));
        }


        public bool HasMeasuredOffset
        {
            get { return  _measuredOffset != null; }
        }



        public List<DrillRackInfo> DrillRack
        {
            get; private set;
        }

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

        private bool _issNavigationMode = true;
        public bool IsNavigationMode
        {
            get { return _issNavigationMode; }
            set { Set(ref _issNavigationMode, value); }
        }

        private bool _isSetFiducialMode = false;
        public bool IsSetFiducialMode
        {
            get { return _isSetFiducialMode; }
            set { Set(ref _isSetFiducialMode, value); }
        }

        private bool _cameraNavigation = false;
        public bool CameraNavigation
        {
            get { return _cameraNavigation; }
            set
            {
                _tool1Navigation = !value;
                _tool2Navigation = false;
                RaisePropertyChanged(nameof(Tool1Navigation));
                RaisePropertyChanged(nameof(Tool2Navigation));
                Set(ref _cameraNavigation, value);

                var currentPoint = new Point2D<double>()
                {
                    X = Machine.NormalizedPosition.X,
                    Y = Machine.NormalizedPosition.Y
                };                

                Machine.GotoPoint(currentPoint);    
            }
        }

        private bool _tool1Navigation = true;
        public bool Tool1Navigation
        {
            get { return _tool1Navigation; }
            set
            {
                _cameraNavigation = !value;
                _tool2Navigation = false;
                RaisePropertyChanged(nameof(CameraNavigation));
                RaisePropertyChanged(nameof(Tool2Navigation));
                Set(ref _tool1Navigation, value);

                if (Machine.Settings.PositioningCamera != null &&
                                   Machine.Settings.PositioningCamera.Tool1Offset != null)
                {
                    var currentPoint = new Point2D<double>()
                    {
                        X = Machine.NormalizedPosition.X - Machine.Settings.PositioningCamera.Tool1Offset.X,
                        Y = Machine.NormalizedPosition.Y - Machine.Settings.PositioningCamera.Tool1Offset.Y
                    };

                    Machine.GotoPoint(currentPoint);
                }
            }
        }

        private bool _tool2Navigation = true;
        public bool Tool2Navigation
        {
            get { return _tool2Navigation; }
            set
            {
                _cameraNavigation = !value;
                _tool1Navigation = false;
                RaisePropertyChanged(nameof(CameraNavigation));
                RaisePropertyChanged(nameof(Tool1Navigation));
                Set(ref _tool2Navigation, value);


                if (Machine.Settings.PositioningCamera != null &&
                    Machine.Settings.PositioningCamera.Tool1Offset != null)
                {
                    var currentPoint = new Point2D<double>()
                    {
                        X = Machine.NormalizedPosition.X - Machine.Settings.PositioningCamera.Tool1Offset.X,
                        Y = Machine.NormalizedPosition.Y - Machine.Settings.PositioningCamera.Tool1Offset.Y
                    };

                    Machine.GotoPoint(currentPoint);
                }
            }
        }
    }
}