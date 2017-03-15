using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LagoVista.GCode.Sender.Application
{
    /// <summary>
    /// A bit of hacking but it's too small/simple to do a view model and I need to get this thing done! (KDW 20170315)
    /// </summary>
    public partial class ConsolidatedDrillBit : Window
    {
        public ConsolidatedDrillBit()
        {
            InitializeComponent();
        }

        bool _isEdit;
        LagoVista.EaglePCB.Models.PCBProject _project; 



        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(DrillName.Text))
            {
                MessageBox.Show("Name must not be empty.");
                DrillName.Focus();
            }

            foreach(var consolidatedDrill in _project.ConsolidatedDrillRack)
            {
                if(consolidatedDrill.NewToolName == DrillName.Text && consolidatedDrill != ConsolidatedDrill)
                {
                    MessageBox.Show("Tool name is in use please select a new one.");
                    DrillName.Text = String.Empty;
                    DrillName.Focus();
                    return;
                }
            }

            double drillDiameter;
            if(!double.TryParse(Diameter.Text, out drillDiameter))
            {
                MessageBox.Show("Please enter a valid drill size (0.3mm - 50mm)");
                Diameter.Text = String.Empty;
                return;
            }

            if (drillDiameter < 0.3 || drillDiameter > 50)
            {
                MessageBox.Show("Diameter must be betwen 0.3mm and 10mm");
                Diameter.Text = String.Empty;
                return;
            }

            ConsolidatedDrill.Diameter = drillDiameter;
            ConsolidatedDrill.NewToolName = DrillName.Text;

            if(!_isEdit)
            {
                _project.ConsolidatedDrillRack.Add(ConsolidatedDrill);
            }

            DialogResult = true;

            Close();
        }

        public LagoVista.EaglePCB.Models.ConsolidatedDrillBit ConsolidatedDrill { get; set; }
        

        public void ShowForNew(LagoVista.EaglePCB.Models.PCBProject project, Window owner)
        {
            _project = project;

            ConsolidatedDrill = new EaglePCB.Models.ConsolidatedDrillBit()
            {
                NewToolName = $"TC{project.ConsolidatedDrillRack.Count + 1:00}"
            };

            DrillName.Text = ConsolidatedDrill.NewToolName;

            this.Owner = owner;
            _isEdit = false;

            this.ShowDialog();
        }

        public void ShowForEdit(LagoVista.EaglePCB.Models.PCBProject project, LagoVista.EaglePCB.Models.ConsolidatedDrillBit drill, Window owner)
        {
            _project = project;

            ConsolidatedDrill = drill;
            this.Owner = owner;
            _isEdit = true;
            DrillName.Text = drill.NewToolName;
            Diameter.Text = drill.Diameter.ToString();

            this.ShowDialog();
        }
    }
}
