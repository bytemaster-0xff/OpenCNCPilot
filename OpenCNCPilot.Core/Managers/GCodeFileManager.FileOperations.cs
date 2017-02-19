using LagoVista.Core.GCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class GCodeFileManager
    {
        public Task OpenFileAsync(string path)
        {
            File = GCodeFile.Load(path);
            if (File != null)
            {
                RenderPaths();
            }
            else
            {
                ClearPaths();
            }

            return Task.FromResult(default(object));
        }

        public Task CloseFileAsync()
        {
            throw new NotImplementedException();
        }
    }
}
