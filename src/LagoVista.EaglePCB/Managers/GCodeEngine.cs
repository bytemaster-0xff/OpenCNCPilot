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

            var allDrills = pcb.Drills.ToList();

            var tools = pcb.Drills.GroupBy(drl => drl.Diameter);
            foreach (var tool in tools)
            {
                if (pcbProject.PauseForToolChange)
                {
                    bldr.AppendLine("M05");
                    bldr.AppendLine($"G0 Z{pcbProject.DrillSafeHeight.ToDim()}");
                    bldr.AppendLine($"M06 {tool.First().Diameter.ToDim()}");
                    bldr.AppendLine($"G0 Z{pcbProject.DrillSafeHeight.ToDim()}");
                    bldr.AppendLine($"G00 X0.0000 Y0.0000");
                    bldr.AppendLine("M03");
                    bldr.AppendLine($"S{pcbProject.DrillSpindleRPM}");
                    bldr.AppendLine($"G04 {pcbProject.DrillSpindleDwell}");
                }

                foreach (var drill in tool)
                {
                    bldr.AppendLine($"G00 X{(drill.X + pcbProject.ScrapSides).ToDim()} Y{(drill.Y + pcbProject.ScrapTopBottom).ToDim()})");
                    bldr.AppendLine($"G01 Z0 F{pcbProject.SafePlungeRecoverRate}");
                    bldr.AppendLine($"G01 Z-{pcbProject.StockThickness.ToDim()} FS{pcbProject.DrillPlungeRate}");
                    bldr.AppendLine($"G01 Z{pcbProject.DrillSafeHeight} F{pcbProject.SafePlungeRecoverRate}");
                }
            }

            bldr.AppendLine("M05");
            bldr.AppendLine("G00 X0 Y0");

            return bldr.ToString();
        }

        /// <summary>
        /// Create GCode that will drill holes to secure the board to the hold down fixture
        /// </summary>
        /// <param name="pcb">PCB Specficiation</param>
        /// <param name="pcbProject">Details about the PCB Project</param>
        /// <param name="drillIntoUnderlayment">If this is true, holes will be drilled into the underlayment or fixture the board is mounted on.  This really only should be done the first time since once holes are created they can be reused and redrilling may result in an undesired offset.</param>
        /// <returns></returns>
        public static string CreateHoldDownGCode(EaglePCB.Models.PCB pcb, PCBProject pcbProject, bool drillIntoUnderlayment)
        {
            var bldr = new StringBuilder();

            bldr.AppendLine("(Metric Mode)");
            bldr.AppendLine("G21");
            bldr.AppendLine("(Absolute Coordinates)");
            bldr.AppendLine("G90");
            bldr.AppendLine("M05");

            var leftDrillX = (pcbProject.ScrapSides - pcbProject.HoldDownBoardOffset);
            var rightDrillX = (pcbProject.ScrapSides + pcbProject.HoldDownBoardOffset + pcb.Width);
            var sideDrillY = (pcbProject.ScrapTopBottom + (pcb.Height / 2));

            var topBottomDrillX = (pcbProject.ScrapSides + (pcb.Width / 2));
            var topDrillY = (pcbProject.ScrapTopBottom + pcb.Height + pcbProject.HoldDownBoardOffset);
            var bottomDrillY = (pcbProject.ScrapTopBottom - pcbProject.HoldDownBoardOffset);


            bldr.AppendLine("M05");
            bldr.AppendLine($"G0 Z{pcbProject.DrillSafeHeight.ToDim()}");
            bldr.AppendLine($"G00 X0.0000 Y0.0000");
            bldr.AppendLine($"M06 {pcbProject.HoldDownDiameter.ToDim()}");
            bldr.AppendLine($"G0 Z{pcbProject.DrillSafeHeight.ToDim()}");

            bldr.AppendLine("M03");
            bldr.AppendLine($"S{pcbProject.DrillSpindleRPM}");
            bldr.AppendLine($"G04 {pcbProject.DrillSpindleDwell}");

            var initialDrillDepth = pcbProject.HoldDownDiameter == pcbProject.HoldDownDrillDiameter && drillIntoUnderlayment ? -pcbProject.HoldDownDrillDepth : -pcbProject.StockThickness;

            bldr.AppendLine($"G00 X{leftDrillX.ToDim()} Y{sideDrillY.ToDim()}");
            bldr.AppendLine($"G01 Z0 F{pcbProject.SafePlungeRecoverRate}");
            bldr.AppendLine($"G01 Z{initialDrillDepth.ToDim()} FS{pcbProject.DrillPlungeRate}");
            bldr.AppendLine($"G01 Z{pcbProject.DrillSafeHeight} F{pcbProject.SafePlungeRecoverRate}");

            bldr.AppendLine($"G00 X{topBottomDrillX.ToDim()} Y{topDrillY.ToDim()}");
            bldr.AppendLine($"G01 Z0 F{pcbProject.SafePlungeRecoverRate}");
            bldr.AppendLine($"G01 Z{initialDrillDepth.ToDim()} FS{pcbProject.DrillPlungeRate}");
            bldr.AppendLine($"G01 Z{pcbProject.DrillSafeHeight} F{pcbProject.SafePlungeRecoverRate}");

            bldr.AppendLine($"G00 X{rightDrillX.ToDim()} Y{sideDrillY.ToDim()}");
            bldr.AppendLine($"G01 Z0 F{pcbProject.SafePlungeRecoverRate}");
            bldr.AppendLine($"G01 Z{initialDrillDepth.ToDim()} FS{pcbProject.DrillPlungeRate}");
            bldr.AppendLine($"G01 Z{pcbProject.DrillSafeHeight} F{pcbProject.SafePlungeRecoverRate}");

            bldr.AppendLine($"G00 X{topBottomDrillX.ToDim()} Y{bottomDrillY.ToDim()}");
            bldr.AppendLine($"G01 Z0 F{pcbProject.SafePlungeRecoverRate}");
            bldr.AppendLine($"G01 Z{initialDrillDepth.ToDim()} FS{pcbProject.DrillPlungeRate}");
            bldr.AppendLine($"G01 Z{pcbProject.DrillSafeHeight} F{pcbProject.SafePlungeRecoverRate}");

            if (pcbProject.HoldDownDiameter != pcbProject.HoldDownDrillDiameter && drillIntoUnderlayment)
            {
                bldr.AppendLine("M05");
                bldr.AppendLine($"G0 Z{pcbProject.DrillSafeHeight.ToDim()}");
                bldr.AppendLine($"G00 X0.0000 Y0.0000");
                bldr.AppendLine($"M06 {pcbProject.HoldDownDrillDiameter.ToDim()}");
                bldr.AppendLine($"G0 Z{pcbProject.DrillSafeHeight.ToDim()}");

                bldr.AppendLine("M03");
                bldr.AppendLine($"S{pcbProject.DrillSpindleRPM}");
                bldr.AppendLine($"G04 {pcbProject.DrillSpindleDwell}");

                bldr.AppendLine($"G00 X{leftDrillX.ToDim()} Y{sideDrillY.ToDim()}");
                bldr.AppendLine($"G01 Z0 F{pcbProject.SafePlungeRecoverRate}");
                bldr.AppendLine($"G01 Z-{pcbProject.HoldDownDrillDepth.ToDim()} FS{pcbProject.DrillPlungeRate}");
                bldr.AppendLine($"G01 Z{pcbProject.DrillSafeHeight} F{pcbProject.SafePlungeRecoverRate}");

                bldr.AppendLine($"G00 X{topBottomDrillX.ToDim()} Y{topDrillY.ToDim()}");
                bldr.AppendLine($"G01 Z0 F{pcbProject.SafePlungeRecoverRate}");
                bldr.AppendLine($"G01 Z-{pcbProject.HoldDownDrillDepth.ToDim()} FS{pcbProject.DrillPlungeRate}");
                bldr.AppendLine($"G01 Z{pcbProject.DrillSafeHeight} F{pcbProject.SafePlungeRecoverRate}");

                bldr.AppendLine($"G00 X{rightDrillX.ToDim()} Y{sideDrillY.ToDim()}");
                bldr.AppendLine($"G01 Z0 F{pcbProject.SafePlungeRecoverRate}");
                bldr.AppendLine($"G01 Z-{pcbProject.HoldDownDrillDepth.ToDim()} FS{pcbProject.DrillPlungeRate}");
                bldr.AppendLine($"G01 Z{pcbProject.DrillSafeHeight} F{pcbProject.SafePlungeRecoverRate}");

                bldr.AppendLine($"G00 X{topBottomDrillX.ToDim()} Y{bottomDrillY.ToDim()}");
                bldr.AppendLine($"G01 Z0 F{pcbProject.SafePlungeRecoverRate}");
                bldr.AppendLine($"G01 Z-{pcbProject.HoldDownDrillDepth.ToDim()} FS{pcbProject.DrillPlungeRate}");
                bldr.AppendLine($"G01 Z{pcbProject.DrillSafeHeight} F{pcbProject.SafePlungeRecoverRate}");
            }

            bldr.AppendLine("M05");
            bldr.AppendLine($"G0 Z{pcbProject.DrillSafeHeight.ToDim()}");
            bldr.AppendLine($"G00 X0.0000 Y0.0000");

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
            double scrapX = pcbProject.ScrapSides;
            double scrapY = pcbProject.ScrapTopBottom;
            double width = pcb.Width;
            double height = pcb.Height;

            var cornerWires = pcb.Layers.Where(layer => layer.Number == 20).FirstOrDefault().Wires.Where(wire => wire.Curve.HasValue == true);

            /* Major hack here */
            var radius = cornerWires.Any() ? Math.Abs(cornerWires.First().Rect.X1 - cornerWires.First().Rect.X2) : 0;

            if (radius == 0)
            {
                var depth = 0.0;
                bldr.AppendLine($"G00 Z{pcbProject.MillSafeHeight}");
                bldr.AppendLine($"G00 X{(scrapX - millRadius).ToDim()} Y{(scrapY - millRadius).ToDim()}");
                bldr.AppendLine($"G00 Z0 F{pcbProject.MillPlungeRate}");

                depth -= pcbProject.MillCutDepth;

                while (depth > -pcbProject.StockThickness)
                {
                    depth = Math.Min(depth, pcbProject.StockThickness);
                    bldr.AppendLine($"G01 Z{depth.ToDim()} F{pcbProject.MillPlungeRate}"); /* Move to cut depth interval at 0,0 */

                    bldr.AppendLine($"G01 X{(scrapX + width + millRadius).ToDim()} Y{(scrapY - millRadius).ToDim()} F{pcbProject.MillFeedRate}"); /* Move to bottom right */
                    bldr.AppendLine($"G01 X{(scrapX + width + millRadius).ToDim()} Y{(scrapY + height + millRadius).ToDim()}"); /* Move to Top Right */
                    bldr.AppendLine($"G01 X{(scrapX - millRadius).ToDim()} Y{(scrapY + height + millRadius).ToDim()}"); /* Move to Top Left */
                    bldr.AppendLine($"G01 X{(scrapX - millRadius).ToDim()} Y{(scrapY - millRadius).ToDim()}"); /* Move back to origin */

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
                bldr.AppendLine($"G00 X{(scrapX + radius).ToDim()} Y{(scrapY - millRadius).ToDim()}");

                bldr.AppendLine($"G01 Z0 F{pcbProject.MillPlungeRate}");

                depth -= pcbProject.MillCutDepth;

                while (depth > -pcbProject.StockThickness)
                {
                    depth = Math.Min(depth, pcbProject.StockThickness);
                    bldr.AppendLine($"G01 Z{depth.ToDim()} F{pcbProject.MillPlungeRate}");   

                    bldr.AppendLine($"G00 X{(scrapX + (width - radius)).ToDim()} Y{(scrapY - millRadius).ToDim()} F{pcbProject.MillFeedRate}"); 

                    bldr.AppendLine($"G03 X{(scrapX + (width + millRadius)).ToDim()} Y{(scrapY + radius).ToDim()} R{radius + millRadius}"); 

                    bldr.AppendLine($"G00 X{(scrapX + (width + millRadius)).ToDim()} Y{(scrapY + (height - radius)).ToDim()}"); 

                    bldr.AppendLine($"G03 X{(scrapX + (width - radius)).ToDim()} Y{(scrapY + (height + millRadius)).ToDim()} R{radius + millRadius}");

                    bldr.AppendLine($"G0 X{(scrapX + radius).ToDim()} Y{(scrapY + (height + millRadius)).ToDim()}"); 

                    bldr.AppendLine($"G03 X{(scrapX - millRadius).ToDim()} Y{(scrapY + (height - radius)).ToDim()} R{radius + millRadius}");

                    bldr.AppendLine($"G0 X{(scrapX - millRadius).ToDim()} Y{(scrapY + radius).ToDim()}"); 

                    bldr.AppendLine($"G03 X{(scrapX + radius).ToDim()} Y{(scrapY - millRadius).ToDim()} R{radius + millRadius}"); 

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
