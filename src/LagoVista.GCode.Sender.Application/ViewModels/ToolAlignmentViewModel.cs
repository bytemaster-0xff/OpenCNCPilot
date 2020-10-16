using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public class ToolAlignmentViewModel : MachineVisionViewModelBase
    {
        public ToolAlignmentViewModel(IMachine machine) : base(machine)
        {

            SetToolOneMovePositionCommand = new RelayCommand(SetTool1MovePosition, () => HasFrame);
            SetToolOnePickPositionCommand = new RelayCommand(SetTool1PickPosition, () => HasFrame);
            SetToolOnePlacePositionCommand = new RelayCommand(SetTool1PlacePosition, () => HasFrame);

            SetToolOneLocationCommand = new RelayCommand(SetTool1Location, () => HasFrame && TopCameraLocation != null);
            SetToolTwoLocationCommand = new RelayCommand(SetTool2Location, () => HasFrame && TopCameraLocation != null);

            SetTopCameraLocationCommand = new RelayCommand(SetTopCameraLocation, () => HasFrame);
            SetBottomCameraLocationCommand = new RelayCommand(SetBottomCameraLocation, () => HasFrame && Machine.Settings.PartInspectionCamera != null);

            SaveCalibrationCommand = new RelayCommand(SaveCalibration, () => IsDirty);
        }
        
        public override async Task InitAsync()
        {
            await base.InitAsync();

            BottomCameraLocation = Machine.Settings.PartInspectionCamera?.AbsolutePosition;
            StartCapture();
        }


        private bool _isDirty = false;
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                Set(ref _isDirty, value);
                SaveCalibrationCommand.RaiseCanExecuteChanged();
            }
        }

        protected override void CaptureStarted()
        {
            SetToolOneLocationCommand.RaiseCanExecuteChanged();
            SetToolTwoLocationCommand.RaiseCanExecuteChanged();
            SetTopCameraLocationCommand.RaiseCanExecuteChanged();
            SetBottomCameraLocationCommand.RaiseCanExecuteChanged();
            SetToolOneMovePositionCommand.RaiseCanExecuteChanged();
            SetToolOnePickPositionCommand.RaiseCanExecuteChanged();
            SetToolOnePlacePositionCommand.RaiseCanExecuteChanged();
        }

        protected override void CaptureEnded()
        {
            SetToolOneLocationCommand.RaiseCanExecuteChanged();
            SetToolTwoLocationCommand.RaiseCanExecuteChanged();
            SetTopCameraLocationCommand.RaiseCanExecuteChanged();
            SetBottomCameraLocationCommand.RaiseCanExecuteChanged();
            SetToolOneMovePositionCommand.RaiseCanExecuteChanged();
            SetToolOnePickPositionCommand.RaiseCanExecuteChanged();
            SetToolOnePlacePositionCommand.RaiseCanExecuteChanged();
        }

        public void SetTopCameraLocation()
        {
            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M75");
            }

            TopCameraLocation = new Point2D<double>(Machine.MachinePosition.X, Machine.MachinePosition.Y);
            SetToolOneLocationCommand.RaiseCanExecuteChanged();
            SetToolTwoLocationCommand.RaiseCanExecuteChanged();
        }

        public void SetBottomCameraLocation()
        {
            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M71");
            }

            Machine.Settings.PartInspectionCamera.AbsolutePosition = new Point2D<double>(Machine.MachinePosition.X, Machine.MachinePosition.Y);
            BottomCameraLocation = Machine.Settings.PartInspectionCamera.AbsolutePosition;

            IsDirty = true;
        }

        public void SetTool1MovePosition()
        {
            Machine.Settings.ToolSafeMoveHeight = Machine.Tool0;

            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M72");
            }

            IsDirty = true;
        }


        public void SetTool1PickPosition()
        {
            Machine.Settings.ToolPickHeight = Machine.Tool0;

            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M73");
            }

            IsDirty = true;
        }

        public void SetTool1PlacePosition()
        {
            Machine.Settings.ToolBoardHeight = Machine.Tool0;

            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M74");
            }

            IsDirty = true;
        }

        public void SetTool1Location()
        {
            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M76");
            }

            Machine.Settings.Tool1Offset = new Point2D<double>()
            {
                X = TopCameraLocation.X - Machine.MachinePosition.X,
                Y = TopCameraLocation.Y - Machine.MachinePosition.Y,
            };

            IsDirty = true;
        }

        public void SetTool2Location()
        {
            if (Machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Machine.SendCommand("M77");
            }

            Machine.Settings.Tool2Offset = new Point2D<double>()
            {
                X = TopCameraLocation.X - Machine.MachinePosition.X,
                Y = TopCameraLocation.Y - Machine.MachinePosition.Y,
            };

            IsDirty = true;
        }

        public async void SaveCalibration()
        {
            await Machine.MachineRepo.SaveAsync();
        }

        public override void CircleLocated(Point2D<double> point, double diameter, Point2D<double> stdDev)
        {
            Machine.BoardAlignmentManager.CircleLocated(point);
        }

        public override void CornerLocated(Point2D<double> point, Point2D<double> stdDev)
        {
            Machine.BoardAlignmentManager.CornerLocated(point);
        }

        public RelayCommand SetBottomCameraLocationCommand { get; private set; }

        public RelayCommand SetToolOneLocationCommand { get; private set; }
        public RelayCommand SetToolTwoLocationCommand { get; private set; }
        public RelayCommand SetTopCameraLocationCommand { get; private set; }

        public RelayCommand SetToolOnePlacePositionCommand { get; private set; }
        public RelayCommand SetToolOneMovePositionCommand { get; private set; }
        public RelayCommand SetToolOnePickPositionCommand { get; private set; }
        public RelayCommand SaveCalibrationCommand { get; private set; }

        Point2D<double> _topCameraLocation;
        public Point2D<double> TopCameraLocation
        {
            get { return _topCameraLocation; }
            set { Set(ref _topCameraLocation, value); }
        }

        Point2D<double> _bottomCameraLocation;
        public Point2D<double> BottomCameraLocation
        {
            get { return _bottomCameraLocation; }
            set { Set(ref _bottomCameraLocation, value); }
        }
    }
}
