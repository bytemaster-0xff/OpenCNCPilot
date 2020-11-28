using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Core.GCode.Commands
{
    public class GCodeLine : GCodeMotion
    {
        public bool Rapid;


        public override double Length
        {
            get { return Delta.Magnitude; }
        }

        public override void ApplyOffset(double x, double y, double z)
        {
            Start = new Vector3(Start.X + x, Start.Y + y, Start.Z + z);
            End = new Vector3(End.X + x, End.Y + y, End.Z + z);
        }

        public override void Rotate(double degrees, Point2D<double> origin = null, Axis axis = Axis.ZAxis, RotateDirection direction = RotateDirection.CounterClockwise)
        {
            if (degrees != 0)
            {
                if(origin == null)
                {
                    origin = new Point2D<double>(0, 0);
                }

                var startPoint = new Point2D<double>(Start.X, Start.Y);
                var endPoint = new Point2D<double>(End.X, End.Y);

                var rotatedStartPoint = startPoint.Rotate(origin, degrees);
                var rotatedEndPoint = endPoint.Rotate(origin, degrees);

                Start = new Vector3(Math.Round(rotatedStartPoint.X, 4), Math.Round(rotatedStartPoint.Y, 4), Start.Z);
                End = new Vector3(Math.Round(rotatedEndPoint.X, 4), Math.Round(rotatedEndPoint.Y, 4), End.Z);
            }
        }

        public override string Line
        {
            get
            {
                var bldr = new StringBuilder();
                bldr.Append(Command);                

                if (End.X != Start.X)
                {
                    bldr.Append($" X{End.X.ToDim()}");
                }

                if (End.Y != Start.Y)
                {
                    bldr.Append($" Y{End.Y.ToDim()}");
                }

                if (End.Z != Start.Z)
                {
                    bldr.Append($" Z{End.Z.ToDim()}");
                }

                if((RotateAngle.HasValue && PreviousRotateAngle.HasValue && RotateAngle != PreviousRotateAngle) ||
                    RotateAngle.HasValue && !PreviousRotateAngle.HasValue)
                {
                    bldr.Append($" E{RotateAngle}");
                }

                if ((Feed.HasValue && PreviousFeed.HasValue && Feed.Value != PreviousFeed.Value) ||
                    Feed.HasValue && !PreviousFeed.HasValue)
                {
                    bldr.Append($" F{Feed.Value}");
                }

                if ((SpindleRPM.HasValue && PreviousSpindleRPM.HasValue && SpindleRPM.Value != PreviousSpindleRPM.Value) ||
                   SpindleRPM.HasValue && !PreviousSpindleRPM.HasValue)
                {
                    bldr.Append($" S{SpindleRPM.Value}");
                }

                return bldr.ToString();
            }
        }

        public override Vector3 Interpolate(double ratio)
        {
            ratio = Math.Min(ratio, 1);
            return Start + Delta * ratio;
        }

        public override IEnumerable<GCodeMotion> Split(double length)
        {
            var divisions = (int)Math.Ceiling(Length / length);

            if (divisions < 1)
                divisions = 1;

            var lastEnd = Start;

            for (int i = 1; i <= divisions; i++)
            {
                var end = Interpolate(((double)i) / divisions);

                var intermediate = this.MemberwiseClone() as GCodeLine;
                intermediate.Start = lastEnd;
                intermediate.End = end;
                intermediate.Feed = Feed;
                intermediate.OriginalLine = $"{OriginalLine} (Prior to Split)";

                yield return intermediate;

                lastEnd = end;
            }
        }
    }
}
