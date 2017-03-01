using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class PCBManager
    {
        public Task OpenFileAsync(string path)
        {
            var doc = XDocument.Load(path);

            this.Board = EaglePCB.Managers.EagleParser.ReadPCB(doc);

            return Task.FromResult(default(object));
        }
    }
}