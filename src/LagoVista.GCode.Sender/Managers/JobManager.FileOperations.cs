using LagoVista.Core.GCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class JobManager
    {
        public Task OpenFileAsync(string path)
        {
            _file = GCodeFile.Load(path);
            if (_file != null)
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
