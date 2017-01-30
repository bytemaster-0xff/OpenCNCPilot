using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.ViewModels;
using LagoVista.GCode.Sender.Models;
using System;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class HeightMapViewModel : GCodeAppViewModel
    {
        HeightMap _currentHeightMap;

        public  HeightMapViewModel(IMachine machine, Settings settings) : base(machine, settings)
        {         
            InitCommanding();
        }        
    }
}
