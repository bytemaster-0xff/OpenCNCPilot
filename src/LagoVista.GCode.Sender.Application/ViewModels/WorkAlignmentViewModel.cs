using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public class WorkAlignmentViewModel : MachineVisionViewModelBase
    {
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
            Machine.BoardAlignmentManager.AlignBoard();
        }

        public override void CircleLocated(Point2D<double> point, double diameter, Point2D<double> stdDev)
        {
            Machine.BoardAlignmentManager.CircleLocated(point);
        }

        public override void CornerLocated(Point2D<double> point, Point2D<double> stdDev)
        {
            Machine.BoardAlignmentManager.CornerLocated(point);
        }

        public RelayCommand AlignBoardCommand { get; private set; }

        public RelayCommand EnabledFiducialPickerCommand { get; private set; }
    }
}
