using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using LagoVista.EaglePCB.Models;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LagoVista.Core;

namespace LagoVista.GCode.Sender.Managers
{
    public class BoardAlignmentManager : Core.Models.ModelBase, IBoardAlignmentManager
    {
        /*   
         *   
         *   
         *   |    |    |
         *   |    |    |_________________________
         *   |    |    PCB Origin
         *   |    |_______________________________
         *   |    Work Origin With Scrap 
         *   |____________________________________
         *   Machine Origin 0, 0
         *   
         *   The Fiducial 1 and 2 offsets will be from the PCB Origin
         *   
         *   The Laser Cutter always has an absolute home position at the bottom left corner
         *   
         *   GRBL has the Machine Origin as the bottom left, but tracks a work location internally
         *   
         *   The laser cutter maintains the work origin in THIS software, not from the firmware.  Prior to sending out 
         *   the GCode the X/Y coord will be offset by the workspace origin on the laser cutter.
         *   
         *   GRBL just works as expected with out maintaining adding an offset prior to sending.
         *   
         *   This routine will set the offset of the stock with respect to the expected offset of the bottom left fiducial.
         *   This may not be the exact bottom left of the physical board but it will be the bottom left of the board
         *   as it was originally placed.
         *   
         *   As an example, let's say the bottom left fiducial was specified as X=7.5, Y=7.5, the machine will accurately 
         *   center the camera over the hole that was dilled originally when the machine was at x=7.5 and y=7.5, it will then 
         *   capture the machine coordinates at that point (with respect to machine origin).
         *   
         *   Once it has the first X, Y machine locations it will add the difference between two fiducuals to the
         *   the current machine location and move to that location.
         *   
         *   Example:
         *    
         *   Step 1) Get Close to Initial Fiducial
         *   Step 2) Machine will jog and close the delta of the X/Y locaton to less than Epsilon (this will be in pixel units)
         *   Step 3) Capture Machine Location os _machineLocationFirstFiducial
         *   Step 4) Use the formula below to determine expected position of Fiducial #2
         *    
         *   machine location = X=42.3  Y=54.3
         *   Fiducial 1         x= 7.5, y= 7.5
         *   Fiducial 2         x=47.5, y=32.5
         *   Difference         x=40.0  y=25.0
         *   
         *   Expected Location of Fiducial #2
         *       x => 42.3 + 40 = 82.3
         *       y => 54.3 + 25 = 79.3
         *   
         *   Step 5) Move to location expected of Fiducial #2
         *   Step 6) Look for Fiducial #2 near X=82.3 Y=79.3
         *   Step 7) Machine will jog and close the delta of the X/Y location to less then Epsilon (again in pixel units)
         *   Step 8) Capture the Machine Location of Fiducual #2
         *
         *   We will pivot around Machine Fiducial Point Number One to find the actual origin.
         * 
         *   
         *   
         */


        /* This is the maximum error in pixels that will be allowed to determine that we have 
         * found the fiducial */
        private const double EPSILON_FIDUCIAL_PIXELS = 2.0;

        /* 
         * Once we move to the second fiducial, we want to see a whole wihtin 10 pixels of the 
         * the center.
         */
        private const double EPSILON_MACHINE_PIXELS = 10;

        /* 
         * When waiting for a machine position we need to be less 
         * than 0.1 mm from the target to say we are there.  In 
         * a perfect world we would specify zero.
         */
        private const double EPSILON_MACHINE_POSITION = 0.1;

        IMachine _machine;
        ILogger _logger;
        IPCBManager _boardManager;


        /* Actual measured when the machine finds the first fiducial */
        Point2D<double> _machineLocationFirstFiducial;

        Point2D<double> _targetLocation;

        /* Actual measured when the machine finds the second fiducial */
        Point2D<double> _machineLocationSecondFiducial;

        List<Point2D<double>> _stablizationList;

        public const int IN_TOLERANCE_COUNT_REQUIRED = 10;

        /* This is the number of consectutive points that are within the
         * tolerance of finding the center of the hole. */
        int _inToleranceCount = 0;

        Timer _timer;

        DateTime _lastEvent;
        DateTime _machinePositionLastUpdated;
        Point2D<double> _machinePosition;

        public enum States
        {
            Idle,
            CenteringFirstFiducial,
            StabilzingAfterFirstFiducialMove,
            MovingToSecondFiducial,
            LocatingSecondFiducial,
            CenteringSecondFiducial,
            StabilzingAfterSecondFiducialMove,
            BoardAlignmentDetermined,
            Failed,
        }

        States _state;

        public BoardAlignmentManager(IMachine machine, ILogger logger, IPCBManager boardManager)
        {
            _machine = machine;
            _logger = logger;
            _boardManager = boardManager;
            _machine.PropertyChanged += _machine_PropertyChanged;
            _stablizationList = new List<Point2D<double>>();

            _timer = new Timer(Timer_Tick, null, 0, 1000);
        }

        public void Timer_Tick(object state)
        {
            switch (_state)
            {

            }
        }

        public void SetNewMachineLocation(Point2D<double> machinePosition)
        {
            _machinePositionLastUpdated = DateTime.Now;
            _machinePosition = machinePosition;

            var isOnTargetLocation = false;

            var deltaX = Math.Abs(machinePosition.X - _targetLocation.X);
            var deltaY = Math.Abs(machinePosition.Y - _targetLocation.Y);
            isOnTargetLocation = (deltaX < EPSILON_MACHINE_POSITION && deltaY < EPSILON_MACHINE_POSITION);

            switch (State)
            {
                case States.MovingToSecondFiducial:
                    if (isOnTargetLocation)
                    {
                        State = States.LocatingSecondFiducial;
                        _lastEvent = DateTime.Now;
                    }
                    break;
                case States.CenteringFirstFiducial:
                    if (isOnTargetLocation)
                    {
                        State = States.StabilzingAfterFirstFiducialMove;
                        _lastEvent = DateTime.Now;
                    }
                    break;
                case States.CenteringSecondFiducial:
                    if(isOnTargetLocation)
                    {
                        State = States.StabilzingAfterSecondFiducialMove;
                        _lastEvent = DateTime.Now;
                    }
                    break;
            }

        }

        public void JogToFindCenter(Point2D<double> machine, Point2D<double> cameraOffsetPixels)
        {
            _lastEvent = DateTime.Now;
        }

        private void _machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_machine.MachinePosition))
            {
                SetNewMachineLocation(new Point2D<double>(_machine.MachinePosition.X, _machine.MachinePosition.Y));
            }
        }

        public void CircleLocated(Point2D<double> cameraOffsetPixels)
        {
            Point2D<double> stablizedPoint = null;

            if (_stablizationList.Any())
            {
                var avgX = _stablizationList.Average(pt => pt.X);
                var avgY = _stablizationList.Average(pt => pt.Y);

                if (cameraOffsetPixels.X - avgX > EPSILON_FIDUCIAL_PIXELS || cameraOffsetPixels.Y - avgY > EPSILON_FIDUCIAL_PIXELS)
                {
                    _stablizationList.Clear();
                }
                else if (_stablizationList.Count > IN_TOLERANCE_COUNT_REQUIRED)
                {
                    stablizedPoint = new Point2D<double>(avgX, avgY);
                }
            }

            _stablizationList.Add(new Point2D<double>(Math.Abs(cameraOffsetPixels.X), Math.Abs(cameraOffsetPixels.Y)));

            switch (_state)
            {
                case States.StabilzingAfterFirstFiducialMove:
                    if (stablizedPoint != null)
                    {
                        if (Math.Abs(stablizedPoint.X) < EPSILON_FIDUCIAL_PIXELS &&
                            Math.Abs(stablizedPoint.X) < EPSILON_FIDUCIAL_PIXELS)
                        {
                            _machineLocationFirstFiducial = _machinePosition;
                            _targetLocation = new Point2D<double>()
                            {
                                X = _machinePosition.X + (_boardManager.SecondFiducial.X - _boardManager.FirstFiducial.X),
                                Y = _machinePosition.Y + (_boardManager.SecondFiducial.X - _boardManager.FirstFiducial.Y),
                            };

                            _machine.SendCommand($"G0 X{_targetLocation.X.ToDim()} Y{_targetLocation.Y.ToDim()}");
                            _state = States.MovingToSecondFiducial;
                            _lastEvent = DateTime.Now;

                        }
                        else
                        {
                            JogToFindCenter(_machinePosition, cameraOffsetPixels);
                            State = States.CenteringFirstFiducial;
                        }
                    }

                    break;

                case States.CenteringFirstFiducial: break;

                case States.LocatingSecondFiducial: break;

                case States.StabilzingAfterSecondFiducialMove:
                    if (stablizedPoint != null)
                    {
                        if (Math.Abs(stablizedPoint.X) < EPSILON_FIDUCIAL_PIXELS &&
                            Math.Abs(stablizedPoint.X) < EPSILON_FIDUCIAL_PIXELS)
                        {
                            _machineLocationSecondFiducial = _machinePosition;
                            _state = States.BoardAlignmentDetermined;
                            _lastEvent = DateTime.Now;
                        }
                        else
                        {
                            JogToFindCenter(_machinePosition, cameraOffsetPixels);
                            State = States.CenteringSecondFiducial;
                        }
                    }
                    break;


                case States.CenteringSecondFiducial: break;
            }
        }

        public Point2D<double> FirstFiducialMachineLocation
        {
            get { return _machineLocationFirstFiducial; }
            set { Set(ref _machineLocationFirstFiducial, value); }
        }

        public Point2D<double> SecondFiducialMachineLocation
        {
            get { return _machineLocationSecondFiducial; }
            set { Set(ref _machineLocationSecondFiducial, value); }
        }

        public States State
        {
            get { return _state; }
            set { Set(ref _state, value); }
        }

        public void CornerLocated(Point2D<double> offsetFromCenter)
        {

        }

        /* This should be called once the camera is realtively close to centered over the first fiducial */
        public void AlignBoard()
        {
            _state = States.CenteringFirstFiducial;


        }
    }
}
