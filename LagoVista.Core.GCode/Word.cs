using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Core.GCode
{
    class Word
    {
        public char Command { get; set; }
        public double Parameter { get; set; }
        public string FullWord { get; set; }
    }
}
