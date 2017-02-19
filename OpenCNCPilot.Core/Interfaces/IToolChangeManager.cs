using LagoVista.Core.GCode.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Interfaces
{
    public interface IToolChangeManager
    {
        Task HandleToolChange(MCode cmd);
    }
}
