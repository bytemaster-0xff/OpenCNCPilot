using LagoVista.Core.Models.Drawing;

namespace LagoVista.Core.GCode.Parser
{
    public class ParserState
    {
        public Vector3 Position;
        public ArcPlane Plane;
        public double? Feed;
        public double? SpindleRPM;
        public double? RotateAngle;
        public ParseDistanceMode DistanceMode;
        public ParseDistanceMode ArcDistanceMode;
        public ParseUnit Unit;
        public double LastMotionMode;

        public ParserState()
        {
            Position = new Vector3();
            Plane = ArcPlane.XY;
            DistanceMode = ParseDistanceMode.Absolute;
            ArcDistanceMode = ParseDistanceMode.Relative;
            Unit = ParseUnit.Metric;
            LastMotionMode = -1;
        }
    }

}
