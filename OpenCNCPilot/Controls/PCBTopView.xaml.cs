using LagoVista.Core.Models.Drawing;
using LagoVista.EaglePCB.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LagoVista.GCode.Sender.Application.Controls
{
    /// <summary>
    /// Interaction logic for PCBTopView.xaml
    /// </summary>
    public partial class PCBTopView : UserControl
    {
        public PCBTopView()
        {
            InitializeComponent();

            var designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!designTime)
            {
                this.DataContextChanged += PCBTopView_DataContextChanged;
            }
        }

        private void PCBTopView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var manager = DataContext as LagoVista.GCode.Sender.Managers.PCBManager;

            if (manager.HasBoard)
            {
                var offsetX = manager.HasProject ? manager.Project.ScrapSides : 0;
                var offsetY = manager.HasProject ? manager.Project.ScrapTopBottom : 0;

                foreach (var drill in manager.Board.Drills)
                {
                    var ellipse = new Ellipse() { Width = drill.Diameter * 10.0, Height = drill.Diameter * 10.0 };
                    ellipse.Fill = Brushes.Black;
                    var x = ((drill.X - (drill.Diameter / 2)) + offsetX);
                    var y = ((manager.Board.Height - (drill.Y + (drill.Diameter / 2))) + offsetY);
                    ellipse.SetValue(Canvas.TopProperty, y * 10);
                    ellipse.SetValue(Canvas.LeftProperty, x * 10);
                    ellipse.ToolTip = $"{x + drill.Diameter / 2}x{y + drill.Diameter / 2} - {drill.Diameter}D";
                    ellipse.Cursor = Cursors.Hand;
                    ellipse.Tag = drill;
                    ellipse.MouseUp += Elipse_MouseUp;
                    BoardLayout.Children.Add(ellipse);
                }

                var outline = new Rectangle();
                outline.Stroke = Brushes.Black;
                outline.StrokeThickness = 2;
                outline.SetValue(Canvas.TopProperty, manager.Project.ScrapTopBottom * 10);
                outline.SetValue(Canvas.LeftProperty, manager.Project.ScrapSides * 10);
                outline.Width = manager.Board.Width * 10;
                outline.Height = manager.Board.Height * 10;

                var cornerWires = manager.Board.Layers.Where(layer => layer.Number == 20).FirstOrDefault().Wires.Where(wire => wire.Curve.HasValue == true);
                var radius = cornerWires.Any() ? Math.Abs(cornerWires.First().Rect.X1 - cornerWires.First().Rect.X2) : 0;
                outline.RadiusX = radius * 10;
                outline.RadiusY = radius * 10;
                BoardLayout.Children.Add(outline);

                BoardLayout.Width = manager.HasProject ? manager.Project.StockWidth * 10 : manager.Board.Width * 10;
                BoardLayout.Height = manager.HasProject ? manager.Project.StockHeight * 10 : manager.Board.Height * 10;

                if (manager.HasProject)
                {
                    foreach(var hole in manager.Project.GetHoldDownDrills(manager.Board))
                    {
                        var ellipse = new Ellipse() { Width = manager.Project.HoldDownDiameter * 10.0, Height = manager.Project.HoldDownDiameter * 10.0 };
                        ellipse.Fill = Brushes.Black;

                        var x = hole.X;
                        var y = manager.Board.Height - hole.Y;

                        ellipse.SetValue(Canvas.TopProperty, (manager.Board.Height - (y + (manager.Project.HoldDownDiameter / 2)))* 10.0);
                        ellipse.SetValue(Canvas.LeftProperty, (x - (manager.Project.HoldDownDiameter / 2)) * 10.0);
                        ellipse.ToolTip = $"{x}x{y} - {manager.Project.HoldDownDiameter}D";
                        ellipse.Cursor = Cursors.Hand;
                        ellipse.Tag = hole;
                        ellipse.MouseUp += Elipse_MouseUp;
                        BoardLayout.Children.Add(ellipse);
                    }
                }
            }
        }

        bool _shouldSetFirstFiducial = true;

        private void Elipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var manager = DataContext as LagoVista.GCode.Sender.Managers.PCBManager;
            var drill = (sender as Ellipse).Tag as Drill;
            if (_shouldSetFirstFiducial)
            {
                manager.FirstFiducial = new Point2D<double>(drill.X, drill.Y);
            }
            else
            {
                manager.SecondFiducial = new Point2D<double>(drill.X, drill.Y);
            }

            _shouldSetFirstFiducial = !_shouldSetFirstFiducial;

        }
    }
}
