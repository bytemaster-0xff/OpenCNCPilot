using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Core.GCode.Commands
{
    public class GCodeArc : GCodeMotion
    {
        public ArcPlane Plane { get; set; }
        public ArcPlane? PreviousPlane { get; set; }
        public ArcDirection Direction;
        public double U;    //absolute position of center in first axis of plane
        public double V;    //absolute position of center in second axis of plane

        public override double Length
        {
            get
            {
                return Math.Abs(AngleSpan * Radius);
            }
        }

        public double StartAngle
        {
            get
            {
                Vector3 StartInPlane = Start.RollComponents(-(int)Plane);
                double X = StartInPlane.X - U;
                double Y = StartInPlane.Y - V;
                return Math.Atan2(Y, X);
            }
        }

        public double EndAngle
        {
            get
            {
                Vector3 EndInPlane = End.RollComponents(-(int)Plane);
                double X = EndInPlane.X - U;
                double Y = EndInPlane.Y - V;
                return Math.Atan2(Y, X);
            }
        }

        public override string Line
        {
            get
            {
                var bldr = new StringBuilder();
                bldr.Append(Command);

                /* Wish I understood this better, arcs can either be expressed as center or as end with a radius, the parser converts
                 * ones that are ends with radius to center (UV), likely have to teset on machine to be 100% certain this works right */

                var center = new Vector3(U, V, 0).RollComponents((int)Plane) - Start;

                if (center.X != 0 && Plane != ArcPlane.YZ)
                {
                    bldr.Append($" I{center.X.ToDim()}");
                }

                if (center.Y != 0 && Plane != ArcPlane.ZX)
                {
                    bldr.Append($" J{center.Y.ToDim()}");
                }

                if (center.Z != 0 && Plane != ArcPlane.XY)
                {
                    bldr.Append($" K{center.Z.ToDim()}");
                }

                if ((Feed.HasValue && PreviousFeed.HasValue && Feed.Value != PreviousFeed.Value) ||
                    Feed.HasValue && !PreviousFeed.HasValue)
                {
                    bldr.Append($" F{Feed.Value}");
                }

                bldr.Append($" X{End.X.ToDim()}");
                bldr.Append($" Y{End.Y.ToDim()}");
                bldr.Append($" Z{End.Z.ToDim()}");

                if ((SpindleRPM.HasValue && PreviousSpindleRPM.HasValue && Feed.Value != PreviousSpindleRPM.Value) ||
                   SpindleRPM.HasValue && !PreviousSpindleRPM.HasValue)
                {
                    bldr.Append($" S{SpindleRPM.Value}");
                }

                return bldr.ToString();
            }
        }


        public double AngleSpan
        {
            get
            {
                double span = EndAngle - StartAngle;

                if (Direction == ArcDirection.CW)
                {
                    if (span >= 0)
                        span -= 2 * Math.PI;
                }
                else
                {
                    if (span <= 0)
                        span += 2 * Math.PI;
                }

                return span;
            }
        }

        public double Radius
        {
            get // get average between both radii
            {
                Vector3 startplane = Start.RollComponents(-(int)Plane);
                Vector3 endplane = End.RollComponents(-(int)Plane);

                return (
                    Math.Sqrt(Math.Pow(startplane.X - U, 2) + Math.Pow(startplane.Y - V, 2)) +
                    Math.Sqrt(Math.Pow(endplane.X - U, 2) + Math.Pow(endplane.Y - V, 2))
                    ) / 2;
            }
        }

        public override Vector3 Interpolate(double ratio)
        {
            double angle = StartAngle + AngleSpan * ratio;

            Vector3 onPlane = new Vector3(U + (Radius * Math.Cos(angle)), V + (Radius * Math.Sin(angle)), 0);

            double helix = (Start + (ratio * Delta)).RollComponents(-(int)Plane).Z;

            onPlane.Z = helix;

            Vector3 interpolation = onPlane.RollComponents((int)Plane);

            return interpolation;
        }

        public override IEnumerable<GCodeMotion> Split(double length)
        {
            var divisions = (int)Math.Ceiling(Length / length);

            if (divisions < 1)
                divisions = 1;

            var previousEnd = Start;

            for (var i = 1; i <= divisions; i++)
            {
                var intermediate = this.MemberwiseClone() as GCodeArc;
                intermediate.Start = previousEnd;
                intermediate.End = Interpolate(((double)i) / divisions);
                intermediate.OriginalLine = $"{OriginalLine} (Prior to Split)"; 
                yield return intermediate;
                previousEnd = intermediate.End;
            }
        }

        public override void ApplyOffset(double x, double y, double z = 0)
        {

        }
    }
}
