using LagoVista.Core.Models.Drawing;
using LagoVista.EaglePCB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Interfaces
{

    public enum BoardAlignmentManagerStates
    {
        Idle,
        EvaluatingInitialAlignment,
        CenteringFirstFiducial,
        StabilzingAfterFirstFiducialMove,
        MovingToSecondFiducial,
        LocatingSecondFiducial,
        CenteringSecondFiducial,
        StabilzingAfterSecondFiducialMove,
        BoardAlignmentDetermined,
        TimedOut,
        Failed,
    }

    public interface IBoardAlignmentManager : IDisposable
    {
        void CornerLocated(Point2D<double> offsetFromCenter);

        void CircleLocated(Point2D<double> offsetFromCenter);

        void SetNewMachineLocation(Point2D<double> newLocation);

        void AlignBoard();

        BoardAlignmentManagerStates State { get; set; }
    }
}
