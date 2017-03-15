using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.EaglePCB.Models
{
    public class DrillBit : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public string ToolName { get; set; }
        public double Diameter { get; set; }

        private string _consolidatedToolName;
        public string ConsolidatedToolName
        {
            get { return _consolidatedToolName; }
            set
            {
                _consolidatedToolName = value;
                PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(ConsolidatedToolName)));
            }
        }
    }
}
