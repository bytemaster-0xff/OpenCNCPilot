using OpenCNCPilot.Core.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCNCPilot.Platform
{
    public class WPFStorage : IStorage
    {
        public bool Exists(string fileName)
        {
            return System.IO.File.Exists(fileName);
        }

        public string ReadAllText(string fullFileName)
        {
            if (System.IO.File.Exists(fullFileName))
            {
                return System.IO.File.ReadAllText(fullFileName);
            }

            return null;
        }

        public List<string> ReadLines(string fullFileName)
        {
            return System.IO.File.ReadAllLines(fullFileName).ToList();
        }

        public void WriteAllLines(string fileName, List<string> contents)
        {
            System.IO.File.WriteAllLines(fileName, contents);
        }

        public void WriteAllText(string fullFileName, string contents)
        {
            System.IO.File.WriteAllText(fullFileName, contents);
        }
    }
}
