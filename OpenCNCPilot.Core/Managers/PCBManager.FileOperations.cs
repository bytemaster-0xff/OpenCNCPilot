using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class PCBManager
    {
        public Task<bool> OpenFileAsync(string path)
        {
            try
            {
                var doc = XDocument.Load(path);

                this.Board = EaglePCB.Managers.EagleParser.ReadPCB(doc);

                return Task.FromResult(true);
            }
            catch(Exception)
            {
                return Task.FromResult(false);
            }
        }
    }
}