using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.Controls
{
    public class ListBoxScroll : System.Windows.Controls.ListBox
    {
        public ListBoxScroll() : base()
        {
            SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(ListBoxScroll_SelectionChanged);
        }

        void ListBoxScroll_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ScrollIntoView(SelectedItem);
        }
    }
}
