using LagoVista.Core.Models.Drawing;


namespace LagoVista.GCode.Sender.Interfaces
{
    public interface IBoardAlignmentPositionManager
    {
        Point2D<double> BoardOriginPoint { get; set; }
        Point2D<double> FirstLocated { get; set; }
        bool HasCalculatedOffset { get; set; }
        Point2D<double> OffsetPoint { get; set; }
        double RotationOffset { get; set; }
        Point2D<double> SecondExpected { get; }
        Point2D<double> SecondLocated { get; set; }

        void Reset();
    }
}