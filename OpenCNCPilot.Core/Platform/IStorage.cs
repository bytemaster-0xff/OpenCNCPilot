using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCNCPilot.Core.Platform
{
    public interface IStorage
    {
        String ReadAllText(String fullFileName);
        void WriteAllText(String fullFileName, String contents);
        List<string> ReadLines(String lines);
        bool Exists(string fileName);
        void WriteAllLines(String fileName, List<String> contents);
    }
}
