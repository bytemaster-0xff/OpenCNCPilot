﻿using LagoVista.Core.Commanding;
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

            SetToolOneLocationCommand = new RelayCommand(SetTool1Location, () => HasFrame);
            SetToolTwoLocationCommand = new RelayCommand(SetTool2Location, () => HasFrame);
            SetTopCameraLocationCommand = new RelayCommand(SetTopCameraLocation, () => HasFrame);
            SetBottomCameraLocationCommand = new RelayCommand(SetBottomCameraLocation, () => HasFrame);

            SaveCalibrationCommand = new RelayCommand(SaveCalibration, () => IsDirty);
        }

        Point2D<double> _tool1Offset;
        Point2D<double> _tool2Offset;
        Point2D<double> _tool1Location;
        Point2D<double> _tool2Location;
        Point2D<double> _topCameraLocation;
        Point2D<double> _bottomCameraLocation;


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
            Machine.SendCommand("M75");
        }

        public void SetBottomCameraLocation()
        {
            Machine.SendCommand("M71");
        }

        public void SetTool1MovePosition()
        {
            Machine.SendCommand("M72");
        }


        public void SetTool1PickPosition()
        {
            Machine.SendCommand("M73");
        }

        public void SetTool1PlacePosition()
        {
            Machine.SendCommand("M74");
        }

        public void SetTool1Location()
        {
            Machine.SendCommand("M76");
        }

        public void SetTool2Location()
        {
            Machine.SendCommand("M77");
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
            await base.InitAsync();

            Tool1Offset = Machine.Settings.PositioningCamera.Tool1Offset;
            Tool2Offset = Machine.Settings.PositioningCamera.Tool2Offset;
            BottomCameraLocation = Machine.Settings.PartInspectionCamera.AbsolutePosition;
            StartCapture();         
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
