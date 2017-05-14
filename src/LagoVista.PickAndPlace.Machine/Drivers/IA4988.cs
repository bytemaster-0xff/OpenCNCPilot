using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoViata.PNP.Drivers
{
    public enum Direction
    {
        Forward,
        Backwards
    }

    public interface IA4988
    {
        bool IsBusy { get; }
        void Start(int steps, double feedRate, Direction direction);
        void Kill();
    }
}
