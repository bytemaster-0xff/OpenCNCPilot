using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MachineControlViewModel
    {
        public enum JogDirections
        {
            YPlus,
            YMinus,
            XPlus,
            XMinus,
            ZPlus,
            ZMinus
        }

        public enum Axis
        {
            XY,
            Z
        }

        public enum Reset
        {
            X,
            Y,
            Z,
            All
        }

        public enum MicroStepModes
        {
            Micro,
            Small,
            Medium,
            Large
        }
    }
}
