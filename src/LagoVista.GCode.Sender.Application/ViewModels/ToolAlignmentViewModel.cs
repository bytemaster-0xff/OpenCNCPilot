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
            SetToolOneLocationCommand = new RelayCommand(SetTool1Location, () => HasFrame);
            SetToolTwoLocationCommand = new RelayCommand(SetTool2Location, () => HasFrame);

            SetTopCameraLocationCommand = new RelayCommand(SetTopCameraLocation, () => HasFrame);
            SetBottomCameraLocationCommand = new RelayCommand(SetBottomCameraLocation, () => HasFrame);

            SaveCalibrationCommand = new RelayCommand(SaveCalibration);
        }

        Point2D<double> _tool1Offset;
        Point2D<double> _tool2Offset;
        Point2D<double> _tool1Location;
        Point2D<double> _tool2Location;
        Point2D<double> _topCameraLocation;
        Point2D<double> _bottomCameraLocation;

        public void SetPlaceHeadLocation()
        {

        }

        protected override void CaptureStarted()
        {
            SetToolOneLocationCommand.RaiseCanExecuteChanged();
            SetToolTwoLocationCommand.RaiseCanExecuteChanged();
            SetTopCameraLocationCommand.RaiseCanExecuteChanged();
            SetBottomCameraLocationCommand.RaiseCanExecuteChanged();
        }

        protected override void CaptureEnded()
        {
            SetToolOneLocationCommand.RaiseCanExecuteChanged();
            SetToolTwoLocationCommand.RaiseCanExecuteChanged();
            SetTopCameraLocationCommand.RaiseCanExecuteChanged();
            SetBottomCameraLocationCommand.RaiseCanExecuteChanged();
        }

        public void SetTopCameraLocation()
        {
            TopCameraLocation = new Point2D<double>(Machine.MachinePosition.X, Machine.MachinePosition.Y);

            if (Tool1Location != null)
            {
                Tool1Offset = new Point2D<double>()
                {
                    X = Tool1Location.X - TopCameraLocation.X,
                    Y = Tool1Location.Y - TopCameraLocation.Y,
                };
            }

            if (Tool2Location != null)
            {
                Tool2Offset = new Point2D<double>()
                {
                    X = Tool2Location.X - TopCameraLocation.X,
                    Y = Tool2Location.Y - TopCameraLocation.Y,
                };
            }

        }

        public void SetBottomCameraLocation()
        {
            BottomCameraLocation = new Point2D<double>(Machine.MachinePosition.X - Tool1Offset.Y, Machine.MachinePosition.Y - Tool1Offset.Y);
        }

        public void SetTool1Location()
        {
            Tool1Location = new Point2D<double>(Machine.MachinePosition.X, Machine.MachinePosition.Y);
            if (TopCameraLocation != null)
            {
                Tool1Offset = new Point2D<double>()
                {
                    X = Tool1Location.X - TopCameraLocation.X,
                    Y = Tool1Location.Y - TopCameraLocation.Y,
                };
            }
        }


        public void SetTool2Location()
        {
            Tool2Location = new Point2D<double>(Machine.MachinePosition.X, Machine.MachinePosition.Y);
            if(TopCameraLocation != null)
            {
                Tool2Offset = new Point2D<double>()
                {
                    X = Tool2Location.X - TopCameraLocation.X,
                    Y = Tool2Location.Y - TopCameraLocation.Y,
                };
            }
        }

        public async void SaveCalibration()
        {
            Machine.Settings.PositioningCamera.Tool1Offset = Tool1Offset;
            Machine.Settings.PositioningCamera.Tool2Offset = Tool2Offset;
            Machine.Settings.PartInspectionCamera.AbsolutePosition = BottomCameraLocation;
            await Machine.MachineRepo.SaveAsync();
        }

        public override async Task InitAsync()
        {
            Tool1Offset = Machine.Settings.PositioningCamera.Tool1Offset;
            Tool2Offset = Machine.Settings.PositioningCamera.Tool2Offset;
            BottomCameraLocation = Machine.Settings.PartInspectionCamera.AbsolutePosition;

            await base.InitAsync();
        }

        public override void CircleLocated(Point2D<double> point, double diameter, Point2D<double> stdDev)
        {
            Machine.BoardAlignmentManager.CircleLocated(point);
        }

        public override void CornerLocated(Point2D<double> point, Point2D<double> stdDev)
        {
            Machine.BoardAlignmentManager.CornerLocated(point);
        }

        public RelayCommand SetToolOneLocationCommand { get; private set; }
        public RelayCommand SetToolTwoLocationCommand { get; private set; }
        public RelayCommand SetTopCameraLocationCommand { get; private set; }
        public RelayCommand SetBottomCameraLocationCommand { get; private set; }
        public RelayCommand SaveCalibrationCommand { get; private set; }

        public Point2D<double> TopCameraLocation
        {
            get { return _topCameraLocation; }
            set { Set(ref _topCameraLocation, value); }
        }

        public Point2D<double> BottomCameraLocation
        {
            get { return _bottomCameraLocation; }
            set { Set(ref _bottomCameraLocation, value); }
        }

        public Point2D<double> Tool1Location
        {
            get { return _tool1Location; }
            set { Set(ref _tool1Location, value); }
        }

        public Point2D<double> Tool2Location
        {
            get { return _tool2Location; }
            set { Set(ref _tool2Location, value); }
        }

        public Point2D<double> Tool1Offset
        {
            get { return _tool1Offset; }
            set { Set(ref _tool1Offset, value); }
        }

        public Point2D<double> Tool2Offset
        {
            get { return _tool2Offset; }
            set { Set(ref _tool2Offset, value); }
        }
    }
}
