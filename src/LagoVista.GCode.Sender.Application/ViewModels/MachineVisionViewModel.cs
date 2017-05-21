using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.EaglePCB.Models;
using LagoVista.GCode.Sender.Interfaces;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public partial class MachineVisionViewModel : MachineVisionViewModelBase
    {

        public MachineVisionViewModel(IMachine machine) : base(machine)
        {
            CaptureCameraCommand = new RelayCommand(CaptureCameraLocation);
            CaptureDrillLocationCommand = new RelayCommand(CaptureDrillLocation);
            AlignBoardCommand = new RelayCommand(AlignBoard, CanAlignBoard);            
        }

        public override async Task InitAsync()
        {
            await base.InitAsync();
            if (Machine.PCBManager.HasBoard)
            {
                PartsList = Machine.PCBManager.Board.Components.OrderBy(cmp => cmp.Name).ToList();
            }            

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

        private Point2D<double> _drillWorkLocation;

        public void AlignBoard()
        {
            Machine.BoardAlignmentManager.AlignBoard();
        }

        public override void CircleLocated(Point2D<double> point, double diameter)
        {
            Machine.BoardAlignmentManager.CircleLocated(point);
        }

        public override void CornerLocated(Point2D<double> point)
        {
            Machine.BoardAlignmentManager.CornerLocated(point);
        }

        public void CaptureDrillLocation()
        {
            if (Machine.Settings.MachineType == FirmwareTypes.GRBL1_1)
            {
                _drillWorkLocation = new Point2D<double>(Machine.NormalizedPosition.X, Machine.NormalizedPosition.Y);
            }
            else
            {
                _drillWorkLocation = new Point2D<double>(Machine.MachinePosition.X, Machine.MachinePosition.Y);
            }
        }

        public async void CaptureCameraLocation()
        {
            if (Machine.Settings.MachineType == FirmwareTypes.GRBL1_1)
            {
                var deltaX = Machine.NormalizedPosition.X - _drillWorkLocation.X;
                var deltaY = Machine.NormalizedPosition.Y - _drillWorkLocation.Y;
                Machine.Settings.PositioningCamera.Tool1Offset = new Point2D<double>(deltaX, deltaY);
            }
            else
            {
                var deltaX = Machine.MachinePosition.X - _drillWorkLocation.X;
                var deltaY = Machine.MachinePosition.Y - _drillWorkLocation.Y;
                Machine.Settings.PositioningCamera.Tool1Offset = new Point2D<double>(deltaX, deltaY);
            }

            await Machine.MachineRepo.SaveAsync();
        }

        Component _selectedComponent;
        public Component SelectedComponent
        {
            set
            {
                Set(ref _selectedComponent, value);
                var point = new Point2D<double>(value.X.Value, value.Y.Value);
                Machine.GotoPoint(point);
            }
            get { return _selectedComponent; }
        }

        List<Component> _partsList;
        public List<Component> PartsList
        {
            get { return _partsList; }
            set { Set(ref _partsList, value); }
        }

        public RelayCommand CaptureDrillLocationCommand { get; private set; }

        public RelayCommand CaptureCameraCommand { get; private set; }

        public RelayCommand AlignBoardCommand { get; private set; }
    }
}