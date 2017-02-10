using LagoVista.GCode.Sender.Interfaces;
using LagoVista.GCode.Sender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class HeightMapProbingManager
    {
        private HeightMap _heightMap;
        public HeightMap HeightMap
        {
            get { return _heightMap; }
            set
            {
                _heightMap = value;
                RaisePropertyChanged();
            }
        }

        public IMachine Machine { get; private set; }
    }
}
