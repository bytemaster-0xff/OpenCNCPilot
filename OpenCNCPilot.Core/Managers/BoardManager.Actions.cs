using LagoVista.EaglePCB.Managers;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class BoardManager
    {
        public Task LoadBoardAsync(string boardFile)
        {
            var tcs = new TaskCompletionSource<object>();
            Task.Run(() =>
            {
                var doc = XDocument.Load(boardFile);
                Board = EagleParser.ReadPCB(doc);
                tcs.SetResult(default(object));
            });

            return tcs.Task;
        }
    }
}
