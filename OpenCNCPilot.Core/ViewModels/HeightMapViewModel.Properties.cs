using LagoVista.GCode.Sender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class HeightMapViewModel
    {
        public HeightMap CurrentHeightMap
        {
            get { return _currentHeightMap; }
            set
            {
                Set(ref _currentHeightMap, value);
                StartProbingCommand.RaiseCanExecuteChanged();
                HeightMapChanged();
            }
        }
    }
}
