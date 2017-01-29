using LagoVista.Core.Models.Drawing;

namespace LagoVista.Core.GCode.Parser
{
    public class ParserState
    {
        public Vector3 Position;
        public ArcPlane Plane;
        public double Feed;
        public ParseDistanceMode DistanceMode;
        public ParseDistanceMode ArcDistanceMode;
        public ParseUnit Unit;
        public int LastMotionMode;

        public ParserState()
        {
            Position = new Vector3();
            Plane = ArcPlane.XY;
            Feed = 100;
            DistanceMode = ParseDistanceMode.Absolute;
            ArcDistanceMode = ParseDistanceMode.Incremental;
            Unit = ParseUnit.Metric;
            LastMotionMode = -1;
        }
    }

}
