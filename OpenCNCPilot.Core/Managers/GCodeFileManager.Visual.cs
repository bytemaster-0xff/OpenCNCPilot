using LagoVista.Core.GCode.Commands;
using System.Collections.Generic;
using System;
using LagoVista.GCode.Sender.Models;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class GCodeFileManager
    {
        private void RenderPaths()
        {
            ClearPaths();

            foreach (var cmd in _file.Commands)
            {
                if (cmd is GCodeLine)
                {
                    var gcodeLine = cmd as GCodeLine;
                    if (gcodeLine.Rapid)
                    {
                        RapidMoves.Add(new Line3D()
                        {
                            Start = gcodeLine.Start,
                            End = gcodeLine.End
                        });
                    }
                    else
                    {
                        Lines.Add(new Line3D()
                        {
                            Start = gcodeLine.Start,
                            End = gcodeLine.End
                        });
                    }
                }

                if (cmd is GCodeArc)
                {
                    var arc = cmd as GCodeArc;
                    var segmentLength = arc.Length / 50;
                    var segments = (cmd as GCodeArc).Split(segmentLength);
                    foreach (var segment in segments)
                    {
                        Arcs.Add(new Line3D()
                        {
                            Start = segment.Start,
                            End = segment.End
                        });
                    }
                }
            }

            RaisePropertyChanged(nameof(Lines));
            RaisePropertyChanged(nameof(RapidMoves));
            RaisePropertyChanged(nameof(Arcs));
        }

        private void ClearPaths()
        {
            Lines.Clear();
            RapidMoves.Clear();
            Arcs.Clear();
        }
    }
}
