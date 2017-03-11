using LagoVista.EaglePCB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.EaglePCB.Managers
{
    public class GCodeEngine
    {
        public static string CreateDrillGCode(EaglePCB.Models.PCB pcb, PCBProject pcbProject)
        {
            var bldr = new StringBuilder();

            bldr.AppendLine("(Metric Mode}");
            bldr.AppendLine("G21");
            bldr.AppendLine("(Absolute Coordinates)");
            bldr.AppendLine("G90");
            bldr.AppendLine("M05");

            var tools = pcb.Drills.GroupBy(drl => drl.Diameter);
            foreach(var tool in tools)
            {
                if (pcbProject.PauseForToolChange)
                {
                    bldr.AppendLine("M05");
                    bldr.AppendLine($"G0 Z{pcbProject.DrillSafeHeight:0.0000}");
                    bldr.AppendLine($"M06 {tool.First().Diameter:0.0000}");
                    bldr.AppendLine($"G0 Z{pcbProject.DrillSafeHeight:0.0000}");
                    bldr.AppendLine($"G00 X0.0000 Y0.0000");
                    bldr.AppendLine("M03");
                    bldr.AppendLine($"S{pcbProject.DrillSpindleRPM}");
                    bldr.AppendLine($"G04 {pcbProject.DrillSpindleDwell}");
                }
                
                foreach (var drill in tool)
                {
                    bldr.AppendLine($"G00 X{(drill.X + pcbProject.Scrap):0.0000} Y{(drill.Y + pcbProject.Scrap):0:0000}");
                    bldr.AppendLine($"G01 Z0 F{pcbProject.SafePlungRecoverRate}");
                    bldr.AppendLine($"G01 Z{pcbProject.BoardDepth:0.0000} FS{pcbProject.DrillPlungRate}");
                    bldr.AppendLine($"G01 Z{pcbProject.DrillSafeHeight} F{pcbProject.SafePlungRecoverRate}");
                }
            }

            bldr.AppendLine("M05");
            bldr.AppendLine("G00 X0 Y0");

            return bldr.ToString();
        }

        public static string CreateCutoutMill(EaglePCB.Models.PCB pcb, PCBProject pcbProject)
        {
            var bldr = new StringBuilder();

            bldr.AppendLine("(Metric Mode}");
            bldr.AppendLine("G21");
            bldr.AppendLine("(Absolute Coordinates)");
            bldr.AppendLine("G90");

            bldr.AppendLine("M03");
            bldr.AppendLine($"S{pcbProject.MillSpindleRPM}");
            bldr.AppendLine($"G04 {pcbProject.MillSpindleDwell}");

            var cornerWires = pcb.Layers.Where(layer => layer.Number == 20).FirstOrDefault().Wires.Where(wire => wire.Curve.HasValue == true);
            var radius = cornerWires.Any() ? Math.Abs(cornerWires.First().Rect.X1 - cornerWires.First().Rect.X2) : 0;
            if (radius == 0)
            {
                var depth = 0;
                while (depth < pcbProject.BoardDepth)
                {
                    bldr.AppendLine($"G00 Z{pcbProject.MillSafeHeight}");
                    bldr.AppendLine($"G00 X{(pcbProject.Scrap - pcbProject.MillToolSize / 2):0.0000} Y{(pcbProject.Scrap - (pcbProject.MillToolSize / 2)):0.0000}");
                    bldr.AppendLine($"G00 X{((pcbProject.Scrap + pcb.Width)- (pcbProject.MillToolSize / 2)):0.0000} Y{(pcbProject.Scrap - (pcbProject.MillToolSize / 2)):0.0000}");
                    bldr.AppendLine($"G00 X{(pcbProject.Scrap - pcbProject.MillToolSize / 2):0.0000} Y{((pcbProject.Scrap + pcb.Height) - pcbProject.MillToolSize / 2):0.0000}");
                }


                boardEdgeMeshBuilder.AddBox(new Point3D(board.Width / 2, board.Height / 2, 0), board.Width, board.Height, 1);
            }
            else
            {
                boardEdgeMeshBuilder.AddBox(new Point3D(board.Width / 2, board.Height / 2, 0), board.Width - (radius * 2), board.Height - (radius * 2), 1);
                boardEdgeMeshBuilder.AddBox(new Point3D(board.Width / 2, radius / 2, 0), board.Width - (radius * 2), radius, 1);
                boardEdgeMeshBuilder.AddBox(new Point3D(board.Width / 2, board.Height - radius / 2, 0), board.Width - (radius * 2), radius, 1);
                boardEdgeMeshBuilder.AddBox(new Point3D(radius / 2, board.Height / 2, 0), radius, board.Height - (radius * 2), 1);
                boardEdgeMeshBuilder.AddBox(new Point3D(board.Width - radius / 2, board.Height / 2, 0), radius, board.Height - (radius * 2), 1);
                boardEdgeMeshBuilder.AddCylinder(new Point3D(radius, radius, -0.5), new Point3D(radius, radius, 0.5), radius, 50, true, true);
                boardEdgeMeshBuilder.AddCylinder(new Point3D(radius, board.Height - radius, -0.5), new Point3D(radius, board.Height - radius, 0.5), radius, 50, true, true);
                boardEdgeMeshBuilder.AddCylinder(new Point3D(board.Width - radius, radius, -0.5), new Point3D(board.Width - radius, radius, 0.5), radius, 50, true, true);
                boardEdgeMeshBuilder.AddCylinder(new Point3D(board.Width - radius, board.Height - radius, -0.5), new Point3D(board.Width - radius, board.Height - radius, 0.50), radius, 50, true, true);
            }


            return bldr.ToString();
        }
    }
}
