using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public class WorkAlignmentViewModel : MachineVisionViewModelBase
    {
        enum BoardAlignmentState
        {
            Idle,
            FindingFiducialOne,
            MovingToSecondFiducial,
            FindingFiducialTwo
        }

        Point2D<double> _fiducialOneLocation;
        Point2D<double> _fiducialTwoLocation;


        BoardAlignmentState _boardAlignmentState = BoardAlignmentState.Idle;

        public WorkAlignmentViewModel(IMachine machine) : base(machine)
        {
            AlignBoardCommand = new RelayCommand(AlignBoard, CanAlignBoard);
            EnabledFiducialPickerCommand = new RelayCommand(() => Machine.PCBManager.IsSetFiducialMode = true);
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

        public void AlignBoard()
        {
            _boardAlignmentState = BoardAlignmentState.FindingFiducialOne;
            Status = "Centering First Fiducial";
        }        

        public override void CircleLocated(Point2D<double> offset, double diameter, Point2D<double> stdDev)
        {
            switch(_boardAlignmentState)
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

        public override void CircleCentered(Point2D<double> point, double diameter)
        {
            switch(_boardAlignmentState)
            {
                case BoardAlignmentState.FindingFiducialOne:
                    Status = "Moving to Second Fiducial";
                    _fiducialOneLocation = new Point2D<double>(Machine.NormalizedPosition.X, Machine.NormalizedPosition.Y);
                    _boardAlignmentState = BoardAlignmentState.MovingToSecondFiducial;
                    var fiducialX = Machine.NormalizedPosition.X + (Machine.PCBManager.SecondFiducial.X - Machine.PCBManager.FirstFiducial.X);
                    var fiducialY = Machine.NormalizedPosition.Y + (Machine.PCBManager.SecondFiducial.Y - Machine.PCBManager.FirstFiducial.Y);
                    Machine.GotoPoint(new Point2D<double>(fiducialX, fiducialY));
                    break;
                case BoardAlignmentState.FindingFiducialTwo:
                    _fiducialTwoLocation = new Point2D<double>(Machine.NormalizedPosition.X, Machine.NormalizedPosition.Y);
                    _boardAlignmentState = BoardAlignmentState.Idle;
                    Status = "Found Second Fiducial";
                    break;
            }
        }

        public override void CornerLocated(Point2D<double> point, Point2D<double> stdDev)
        {
            Machine.BoardAlignmentManager.CornerLocated(point);
        }

        public RelayCommand AlignBoardCommand { get; private set; }

        public RelayCommand EnabledFiducialPickerCommand { get; private set; }

        private string _status = "Idle";
        public string Status
        {
            get { return _status; }
            set { Set(ref _status, value); }
        }
    }
}
