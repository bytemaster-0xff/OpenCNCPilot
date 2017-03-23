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

        public override void ApplyOffset(double x, double y, double z, double angle)
        {
            Start = new Vector3(Start.X + x, Start.Y + y, Start.Z + z);
            End = new Vector3(End.X + x, End.Y + y, End.Z + z);
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

                if((Feed.HasValue && PreviousFeed.HasValue && Feed.Value != PreviousFeed.Value) || 
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
            int divisions = (int)Math.Ceiling(Length / length);

            if (divisions < 1)
                divisions = 1;

            Vector3 lastEnd = Start;

            for (int i = 1; i <= divisions; i++)
            {
                Vector3 end = Interpolate(((double)i) / divisions);

                var immediate = new GCodeLine();
                immediate.Start = lastEnd;
                immediate.End = end;
                immediate.Feed = Feed;

                yield return immediate;

                lastEnd = end;
            }
        }
    }
}
