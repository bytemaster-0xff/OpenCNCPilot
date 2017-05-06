using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoViata.PNP.Channel
{
    public interface IServer
    {
        Task StartListeningAsync();

        Connection Connection { get; }
    }
}
