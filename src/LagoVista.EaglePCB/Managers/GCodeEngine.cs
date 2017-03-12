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

            bldr.AppendLine("(Metric Mode)");
            bldr.AppendLine("G21");
            bldr.AppendLine("(Absolute Coordinates)");
            bldr.AppendLine("G90");
            bldr.AppendLine("M05");

            var tools = pcb.Drills.GroupBy(drl => drl.Diameter);
            foreach (var tool in tools)
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

            bldr.AppendLine("(Metric Mode)");
            bldr.AppendLine("G21");
            bldr.AppendLine("(Absolute Coordinates)");
            bldr.AppendLine("G90");

            bldr.AppendLine("M03");
            bldr.AppendLine($"S{pcbProject.MillSpindleRPM}");
            bldr.AppendLine($"G04 {pcbProject.MillSpindleDwell}");

            double millRadius = pcbProject.MillToolSize / 2;
            double scrap = pcbProject.Scrap;
            double width = pcb.Width;
            double height = pcb.Height;

            var cornerWires = pcb.Layers.Where(layer => layer.Number == 20).FirstOrDefault().Wires.Where(wire => wire.Curve.HasValue == true);

            /* Major hack here */
            var radius = cornerWires.Any() ? Math.Abs(cornerWires.First().Rect.X1 - cornerWires.First().Rect.X2) : 0;

            if (radius == 0)
            {
                var depth = 0.0;
                bldr.AppendLine($"G00 Z{pcbProject.MillSafeHeight}");
                bldr.AppendLine($"G00 X{(scrap - millRadius):0.0000} Y{(scrap - millRadius):0.0000}");
                bldr.AppendLine($"G00 Z{pcbProject.MillPlungeRate}");

                depth -= pcbProject.MillCutDepth;

                while (depth > -pcbProject.BoardDepth)
                {
                    depth = Math.Min(depth, pcbProject.BoardDepth);
                    bldr.AppendLine($"G01 Z{depth:0.0000} F{pcbProject.MillPlungeRate}"); /* Move to cut depth interval at 0,0 */

                    bldr.AppendLine($"G01 X{(scrap + width + millRadius):0.0000} Y{(scrap - millRadius):0.0000} F{pcbProject.MillFeedRate}"); /* Move to bottom right */
                    bldr.AppendLine($"G01 X{(scrap + width + millRadius):0.0000} Y{(scrap + height + millRadius):0.0000}"); /* Move to Top Right */
                    bldr.AppendLine($"G01 X{(scrap - millRadius):0.0000} Y{(scrap + height + millRadius):0.0000}"); /* Move to Top Left */
                    bldr.AppendLine($"G01 X{(scrap - millRadius):0.0000} Y{(scrap - millRadius):0.0000}"); /* Move back to origin */

                    depth -= pcbProject.MillCutDepth;
                }

                bldr.AppendLine($"G00 Z{pcbProject.MillSafeHeight}");
                bldr.AppendLine("M05");
                bldr.AppendLine("G0 X0 Y0");
            }
            else
            {

                var depth = 0.0;
                depth += pcbProject.MillCutDepth;
                bldr.AppendLine($"G00 Z{pcbProject.MillSafeHeight}");
                bldr.AppendLine($"G00 X{(scrap + radius):0.0000} Y{(scrap - millRadius):0.0000}");

                bldr.AppendLine($"G01 Z0 F{pcbProject.MillPlungeRate}");

                depth -= pcbProject.MillCutDepth;

                while (depth > -pcbProject.BoardDepth)
                {
                    depth = Math.Min(depth, pcbProject.BoardDepth);
                    bldr.AppendLine($"G01 Z{depth:0.0000} F{pcbProject.MillPlungeRate}");   

                    bldr.AppendLine($"G00 X{(scrap + (width - radius)):0.0000} Y{(scrap - millRadius):0.0000} F{pcbProject.MillFeedRate}"); 


                    bldr.AppendLine($"G03 X{(scrap + (width + millRadius)):0.0000} Y{(scrap + radius):0.0000} R{radius + millRadius}"); 

                    bldr.AppendLine($"G00 X{(scrap + (width + millRadius)):0.0000} Y{(scrap + (height - radius)):0.0000}"); 

                    bldr.AppendLine($"G03 X{(scrap + (width - radius)):0.0000} Y{(scrap + (height + millRadius)):0.0000} R{radius + millRadius}");

                    bldr.AppendLine($"G0 X{(scrap + radius):0.0000} Y{(scrap + (height + millRadius)):0.0000}"); 

                    bldr.AppendLine($"G03 X{(scrap - millRadius):0.0000} Y{(scrap + (height - radius)):0.0000} R{radius + millRadius}");

                    bldr.AppendLine($"G0 X{(scrap - millRadius):0.0000} Y{(scrap + radius):0.0000}"); 

                    bldr.AppendLine($"G03 X{(scrap + radius):0.0000} Y{(scrap - millRadius):0.0000} R{radius + millRadius}"); 

                    depth -= pcbProject.MillCutDepth;
                }

                bldr.AppendLine($"G00 Z{pcbProject.MillSafeHeight}");
                bldr.AppendLine("M05");
                bldr.AppendLine("G0 X0 Y0");
            }


            return bldr.ToString();
        }
    }
}
