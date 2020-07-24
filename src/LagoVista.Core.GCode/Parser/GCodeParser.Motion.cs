using LagoVista.Core.GCode.Commands;
using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Core.GCode.Parser
{
    public partial class GCodeParser
    {
        public GCodeArc ParseArc(List<Word> words, double motionMode, Vector3 EndPos, double UnitMultiplier)
        {
            double U, V;

            bool IJKused = false;

            switch (State.Plane)
            {
                default:
                    U = State.Position.X;
                    V = State.Position.Y;
                    break;
                case ArcPlane.YZ:
                    U = State.Position.Y;
                    V = State.Position.Z;
                    break;
                case ArcPlane.ZX:
                    U = State.Position.Z;
                    V = State.Position.X;
                    break;
            }

            int ArcIncremental = (State.ArcDistanceMode == ParseDistanceMode.Relative) ? 1 : 0;

            for (int i = 0; i < words.Count; i++)
            {
                if (words[i].Command != 'I')
                    continue;

                switch (State.Plane)
                {
                    case ArcPlane.XY:
                        U = words[i].Parameter * UnitMultiplier + ArcIncremental * State.Position.X;
                        break;
                    case ArcPlane.YZ:
                        throw new Exception("Current Plane is YZ, I word is invalid");
                    case ArcPlane.ZX:
                        V = words[i].Parameter * UnitMultiplier + ArcIncremental * State.Position.X;
                        break;
                }

                IJKused = true;
                words.RemoveAt(i);
                break;
            }

            for (int i = 0; i < words.Count; i++)
            {
                if (words[i].Command != 'J')
                    continue;

                switch (State.Plane)
                {
                    case ArcPlane.XY:
                        V = words[i].Parameter * UnitMultiplier + ArcIncremental * State.Position.Y;
                        break;
                    case ArcPlane.YZ:
                        U = words[i].Parameter * UnitMultiplier + ArcIncremental * State.Position.Y;
                        break;
                    case ArcPlane.ZX:
                        throw new Exception("Current Plane is ZX, J word is invalid");
                }

                IJKused = true;
                words.RemoveAt(i);
                break;
            }

            for (int i = 0; i < words.Count; i++)
            {
                if (words[i].Command != 'K')
                    continue;

                switch (State.Plane)
                {
                    case ArcPlane.XY:
                        throw new Exception("Current Plane is XY, K word is invalid");
                    case ArcPlane.YZ:
                        V = words[i].Parameter * UnitMultiplier + ArcIncremental * State.Position.Z;
                        break;
                    case ArcPlane.ZX:
                        U = words[i].Parameter * UnitMultiplier + ArcIncremental * State.Position.Z;
                        break;
                }

                IJKused = true;
                words.RemoveAt(i);
                break;
            }

            for (int i = 0; i < words.Count; i++)
            {
                if (words[i].Command != 'R')
                    continue;

                if (IJKused)
                    throw new Exception("Both IJK and R notation used");

                if (State.Position == EndPos)
                    throw new Exception("arcs in R-notation must have non-coincident start and end points");

                double Radius = words[i].Parameter * UnitMultiplier;

                if (Radius == 0)
                    throw new Exception("Radius can't be zero");

                double A, B;

                switch (State.Plane)
                {
                    default:
                        A = EndPos.X;
                        B = EndPos.Y;
                        break;
                    case ArcPlane.YZ:
                        A = EndPos.Y;
                        B = EndPos.Z;
                        break;
                    case ArcPlane.ZX:
                        A = EndPos.Z;
                        B = EndPos.X;
                        break;
                }

                A -= U;     //(AB) = vector from start to end of arc along the axes of the current plane
                B -= V;

                var C = Math.Round(A, 4) == Math.Round(B, 4) ? A : -A;
                var D = Math.Round(A, 4) == Math.Round(B, 4) ? -B : B;

                {   //normalize perpendicular vector
                    double perpLength = Math.Sqrt(C * C + D * D);
                    C /= perpLength;
                    D /= perpLength;
                }

                var PerpSquare = (Radius * Radius) - ((A * A + B * B) / 4);

                if (PerpSquare < 0)
                    throw new Exception("arc radius too small to reach both ends");

                double PerpLength = Math.Sqrt(PerpSquare);

                if (motionMode == 3 ^ Radius < 0)
                    PerpLength = -PerpLength;

                U += (A / 2) + (C * PerpLength);
                V += (B / 2) + (D * PerpLength);

                words.RemoveAt(i);
                break;
            }

            var arc = new GCodeArc();
            arc.Start = State.Position;
            arc.End = EndPos;
            arc.Direction = (motionMode == 2) ? ArcDirection.CW : ArcDirection.CCW;
            arc.U = U;
            arc.V = V;
            arc.Plane = State.Plane;

            return arc;

        }

        private GCodeMotion ParseLine(List<Word> Words, double MotionMode, Vector3 EndPos)
        {
            if (Words.Count > 0)
            {
                var bldr = new StringBuilder();
                foreach (var word in Words)
                {
                    bldr.Append(word.FullWord + "; ");
                }

                throw new Exception("Motion Command must be last in line Unparsed words in block => " + bldr.ToString());
            }

            var motion = new GCodeLine();
            motion.Start = State.Position;
            motion.End = EndPos;
            motion.Rapid = MotionMode == 0;
            return motion;
        }
    }
}
