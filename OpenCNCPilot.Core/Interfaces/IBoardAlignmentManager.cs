using LagoVista.Core.Models.Drawing;
using LagoVista.EaglePCB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Interfaces
{
    public interface IBoardAlignmentManager
    {
        void CornerLocated(Point2D<double> offsetFromCenter);

        void CircleLocated(Point2D<double> offsetFromCenter);

        void AlignBoard();
    }
}
