using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.GCode.Sender.Managers;
using System;
using System.Threading;
using System.Threading.Tasks;
using LagoVista.Core;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public class WorkAlignmentViewModel : MachineVisionViewModelBase
    {
        IBoardAlignmentPositionManager _positionManager;

        enum BoardAlignmentState
        {
            Idle,
            FindingFiducialOne,
            MovingToSecondFiducial,
            FindingFiducialTwo,
            MovingToOrigin,
        }

        Timer _timer;

        BoardAlignmentState _boardAlignmentState = BoardAlignmentState.Idle;

        DateTime _lastActivity;

        public WorkAlignmentViewModel(IMachine machine) : base(machine)
        {
            AlignBoardCommand = new RelayCommand(AlignBoard, CanAlignBoard);
            CancelBoardAlignmentCommand = new RelayCommand(CancelBoardAlignment, CanCancelBoardAlignment);
            EnabledFiducialPickerCommand = new RelayCommand(() => Machine.PCBManager.IsSetFiducialMode = true);

            _positionManager = new BoardAlignmentPositionManager(Machine.PCBManager);
            _timer = new Timer(Timer_Tick, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Timer_Tick(object state)
        {
            switch(_boardAlignmentState)
            {
                case BoardAlignmentState.MovingToOrigin:
                    if(!Machine.Busy)
                    {
                        Machine.SendCommand("G10 L20 P0 X0 Y0");
                        _boardAlignmentState = BoardAlignmentState.Idle;
                        Status = "Completed";
                    }
                    break;
                case BoardAlignmentState.Idle:
                    break;
                default:
                    if((DateTime.Now - _lastActivity).TotalSeconds > 5)
                    {
                        _boardAlignmentState = BoardAlignmentState.Idle;
                        Status = "Timeout";
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                    break;

            }
        }

        public override async Task InitAsync()
        {
            await base.InitAsync();

            Machine.PropertyChanged += Machine_PropertyChanged;
            Machine.PCBManager.PropertyChanged += PCBManager_PropertyChanged;
        }

        private void PCBManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            AlignBoardCommand.RaiseCanExecuteChanged();
        }

        private void Machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            AlignBoardCommand.RaiseCanExecuteChanged();
        }

        public bool CanAlignBoard()
        {
            return Machine.Mode == OperatingMode.Manual &&
                   Machine.PCBManager.HasBoard &&
                   Machine.PCBManager.FirstFiducial != null &&
                   Machine.PCBManager.SecondFiducial != null;
        }

        public bool CanCancelBoardAlignment()
        {
            return _boardAlignmentState != BoardAlignmentState.Idle;
        }

        public void CancelBoardAlignment()
        {
            _boardAlignmentState = BoardAlignmentState.Idle;
            Status = "Idle";
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void AlignBoard()
        {
            _lastActivity = DateTime.Now;

            _boardAlignmentState = BoardAlignmentState.FindingFiducialOne;
            Status = "Centering On First Fiducial";

            _timer.Change(0, 500);
        }

        public override void CircleLocated(Point2D<double> offset, double diameter, Point2D<double> stdDev)
        {
            if (!Machine.Busy)
            {
                _lastActivity = DateTime.Now;

                switch (_boardAlignmentState)
                {
                    case BoardAlignmentState.FindingFiducialOne:
                    case BoardAlignmentState.FindingFiducialTwo:
                        JogToLocation(offset);
                        break;
                    case BoardAlignmentState.MovingToSecondFiducial:
                        Status = "Searching for Second Fiducial";
                        _boardAlignmentState = BoardAlignmentState.FindingFiducialTwo;
                        break;
                }
            }
        }
       
        public override void CircleCentered(Point2D<double> point, double diameter)
        {
            if (!Machine.Busy)
            {
                switch (_boardAlignmentState)
                {
                    case BoardAlignmentState.FindingFiducialOne:
                        _lastActivity = DateTime.Now;
                        Status = "Moving to Second Fiducial";
                        _positionManager.FirstLocated = RequestedPosition;
                        _boardAlignmentState = BoardAlignmentState.MovingToSecondFiducial;

                        var fiducialX = RequestedPosition.X + (Machine.PCBManager.SecondFiducial.X - Machine.PCBManager.FirstFiducial.X);
                        var fiducialY = RequestedPosition.Y + (Machine.PCBManager.SecondFiducial.Y - Machine.PCBManager.FirstFiducial.Y);                        
                        RequestedPosition = new Point2D<double>(fiducialX, fiducialY);                        

                        Machine.GotoPoint(RequestedPosition);

                        break;
                    case BoardAlignmentState.FindingFiducialTwo:
                        _lastActivity = DateTime.Now;
                        _positionManager.SecondLocated = RequestedPosition;
                        _boardAlignmentState = BoardAlignmentState.MovingToOrigin;
                        Machine.PCBManager.SetMeasuredOffset(_positionManager.OffsetPoint, _positionManager.RotationOffset.ToDegrees());
                        Machine.GotoPoint(_positionManager.OffsetPoint);
                        Status = "Returning to Board Origin";
                        break;
                }
            }
        }

        public override void CornerLocated(Point2D<double> point, Point2D<double> stdDev)
        {
            Machine.BoardAlignmentManager.CornerLocated(point);
        }

        public RelayCommand AlignBoardCommand { get; private set; }

        public RelayCommand CancelBoardAlignmentCommand { get; private set; }

        public RelayCommand EnabledFiducialPickerCommand { get; private set; }

        private string _status = "Idle";
        public string Status
        {
            get { return _status; }
            set { Set(ref _status, value); }
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}
