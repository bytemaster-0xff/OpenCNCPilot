using LagoVista.Core.GCode.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Core.GCode.Parser
{
    public interface IGCodeParser
    {
        GCodeCommand ParseLine(string line, int index);
    }
}
