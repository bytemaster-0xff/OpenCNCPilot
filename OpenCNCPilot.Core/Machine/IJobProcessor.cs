using LagoVista.Core.GCode.Commands;
using LagoVista.GCode.Sender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender
{
    public interface IJobProcessor
    {
        int CurrentIndex { get; }
        int TotalLines { get; }

        void Process();

        GCodeCommand CurrentCommand { get; }

        int CommandAcknowledged();

        bool Completed { get; }

        void Reset();

        TimeSpan TimeRemaining { get; }
        TimeSpan ElapsedTime { get; }
        DateTime EstimatedCompletion { get; }

        void ApplyHeightMap(HeightMap map);

        void ArcToLines(double length);

        IEnumerable<GCodeCommand> Commands { get; }

    }
}
