﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
                    var elipse = new Ellipse() { Width = drill.Diameter * 10.0, Height = drill.Diameter * 10.0 };
                    elipse.Fill = Brushes.Black;
                    var x = ((drill.X - (drill.Diameter / 2)) + offsetX);
                    var y = ((manager.Board.Height - (drill.Y + (drill.Diameter / 2))) + offsetY);
                    elipse.SetValue(Canvas.TopProperty, y * 10);
                    elipse.SetValue(Canvas.LeftProperty, x * 10);
                    elipse.ToolTip = $"{x + drill.Diameter / 2}x{y + drill.Diameter / 2} - {drill.Diameter}D";

                    BoardLayout.Children.Add(elipse);
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
                        var elipse = new Ellipse() { Width = manager.Project.HoldDownDiameter * 10.0, Height = manager.Project.HoldDownDiameter * 10.0 };
                        elipse.Fill = Brushes.Black;
                        elipse.SetValue(Canvas.TopProperty, (hole.Y- (manager.Project.HoldDownDiameter / 2))* 10.0);
                        elipse.SetValue(Canvas.LeftProperty, (hole.X - (manager.Project.HoldDownDiameter / 2)) * 10.0);
                        elipse.ToolTip = $"{hole.X}x{hole.Y} - {manager.Project.HoldDownDiameter}D";
                        BoardLayout.Children.Add(elipse);
                    }
                }
            }
        }
    }
}
