using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.EaglePCB.Models
{
    public class ConsolidatedDrillBit : INotifyPropertyChanged
    {
        public ConsolidatedDrillBit()
        {
            Bits = new ObservableCollection<DrillBit>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public String NewToolName { get; set; }

        public double Diameter { get; set; }

        public ObservableCollection<DrillBit> Bits { get; set; }

        public string Display
        {
            get
            {
                var bldr = new StringBuilder();
                bldr.Append($"{NewToolName} {Diameter:0.00}mm ");
                foreach(var bit in Bits)
                {
                    bldr.Append($"{bit.ToolName} {bit.Diameter:0.00}mm;");
                }

                return bldr.ToString();
            }
        }        

        public string Title
        {
            get { return $"{NewToolName} ({Diameter:0.00}) mm"; }
        }

        public void AddBit(DrillBit bit)
        {
            Bits.Add(bit);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Bits)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Display)));
        }

        public void RemoveBit(DrillBit bit)
        {
            var selectedBit = Bits.Where(bt => bt.ToolName == bit.ToolName).FirstOrDefault();
            if (selectedBit != null)
            {
                Bits.Remove(selectedBit);
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Bits)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Display)));
        }
    }
}
