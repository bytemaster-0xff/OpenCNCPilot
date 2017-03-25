using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public class BoardAlignmentManager
    {
        IMachine _machine;
        ILogger _logger;
        IPCBManager _boardManager;
        Point2D<double> _machineLocationFirstFiducial;
        Point2D<double> _machineLocationSecondFiducial;

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


        public void AlignBoard()
        {


        }




    }
}
