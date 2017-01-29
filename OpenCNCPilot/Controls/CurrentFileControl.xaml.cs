using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace LagoVista.GCode.Sender.Application.Controls
{
    /// <summary>
    /// Interaction logic for CurrentFileStatus.xaml
    /// </summary>
    public partial class CurrentFileControl : UserControl
    {
        public CurrentFileControl()
        {
            InitializeComponent();
        }
        
        public IJobProcessor CurrentJob
        {
            get { return App.Current.Machine.CurrentJob; }
        }
    }
}
