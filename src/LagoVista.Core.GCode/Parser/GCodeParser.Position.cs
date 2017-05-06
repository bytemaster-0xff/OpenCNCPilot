using LagoVista.Core.Models.Drawing;
using System.Collections.Generic;

namespace LagoVista.Core.GCode.Parser
{
    public partial class GCodeParser
    {
        private Vector3 FindEndPosition(List<Word> words, double unitMultiplier)
        {
            Vector3 EndPos = State.Position;

            int Incremental = (State.DistanceMode == ParseDistanceMode.Incremental) ? 1 : 0;

            for (int i = 0; i < words.Count; i++)
            {
                if (words[i].Command != 'X')
                    continue;
                EndPos.X = words[i].Parameter * unitMultiplier + Incremental * EndPos.X;
                words.RemoveAt(i);
                break;
            }

            for (int i = 0; i < words.Count; i++)
            {
                if (words[i].Command != 'Y')
                    continue;
                EndPos.Y = words[i].Parameter * unitMultiplier + Incremental * EndPos.Y;
                words.RemoveAt(i);
                break;
            }

            for (int i = 0; i < words.Count; i++)
            {
                if (words[i].Command != 'Z')
                    continue;
                EndPos.Z = words[i].Parameter * unitMultiplier + Incremental * EndPos.Z;
                words.RemoveAt(i);
                break;
            }

            return EndPos;
        }
    }
}
