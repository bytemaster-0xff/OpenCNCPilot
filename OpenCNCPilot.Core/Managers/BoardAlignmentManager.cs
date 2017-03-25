using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using LagoVista.EaglePCB.Models;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
         *   
         */ 


        IMachine _machine;
        ILogger _logger;
        IPCBManager _boardManager;
        Point2D<double> _machineLocationFirstFiducial;
        Point2D<double> _machineLocationSecondFiducial;

        public enum States
        {
            Idle,
            CenteringFirstFiducial,
            StabilzingAfterFirstFiducialMove,
            MovingToSecondFiducial,
            CenteringOverSecondFiducial,
            StabilzingAfterSecondFiducialMove,
            BoardAlignmentDetermined,
            Failed,
        }



        public BoardAlignmentManager(IMachine machine, ILogger logger, IPCBManager boardManager)
        {
            _machine = machine;
            _logger = logger;
            _boardManager = boardManager;
        }

        public void CircleLocated(Point2D<double> offsetFromCenter)
        {

        }

        public void CornerLocated(Point2D<double> offsetFromCenter)
        {

        }


        /* This should be called once the camera is realtively close to centered over the first fiducial */ 
        public void AlignBoard()
        {


        }

    }
}
