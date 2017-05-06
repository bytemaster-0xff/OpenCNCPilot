using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoViata.PNP.Services
{
    public interface IAxis
    {
        double WorkOffset { get; }
        double CurrentLocation { get; }
        void Move(double position, double feed);
        bool IsBusy { get; }
        void Kill();
        void Home();
        bool MinEndStopTrigger { get; }
        bool MaxEndStopTrigger { get; }
        void Update(long uSeconds);
    }
}
