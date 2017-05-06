using LagoVista.Core.GCode.Commands;
using LagoVista.Core.GCode.Parser;
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Core.GCode
{
    public partial class GCodeFile
    {
        public static GCodeFile Load(string path, ILogger logger = null)
        {
            var parser = new GCodeParser(logger == null ? Services.Logger : logger);
            parser.Reset();
            parser.ParseFile(path);

            return new GCodeFile(parser.Commands) { FileName = path.Substring(path.LastIndexOf('\\') + 1) };
        }

        public static GCodeFile FromList(IEnumerable<string> file, ILogger logger = null)
        {
            var parser = new GCodeParser(logger == null ? Services.Logger : logger);
            parser.Reset();
            parser.Parse(file);

            return new GCodeFile(parser.Commands) { FileName = "output.nc" };
        }

        public static GCodeFile FromString(String contents, ILogger logger = null)
        {
            var commands = contents.Split('\n');
            return FromList(commands, logger);
        }

        public static GCodeFile FromCommands(List<GCodeCommand> commands)
        {
            return new GCodeFile(commands) { FileName = "output.nc" };
        }

        public static GCodeFile GetEmpty()
        {
            return new GCodeFile(new List<GCodeCommand>());
        }
    }
}
