using System;
using System.Linq;
using System.Text;
using LagoVista.EaglePCB;
using LagoVista.GCode.Sender;

namespace LagoVista.PickAndPlace.Managers
{
    public class Planner
    {
        public String CreatePlan(Machine machine, Models.PnPJob job, EaglePCB.Models.PCB board)
        {
            var bldr = new StringBuilder();

            bldr.Append($"M62 P1"); /* Turn on Vacuum */

            foreach(var partType in job.Parts)
            {
                var feeder = job.Feeders.Where(fdr => fdr.Rows.Where(rw => rw.Part.PartNumber == partType.PartNumber).Any()).FirstOrDefault();
                if (feeder != null)
                {
                    var row = feeder.Rows.Where(rw => rw.Part.PartNumber == partType.PartNumber).FirstOrDefault();
                    var parts = board.Components.Where(prt => prt.LibraryName == partType.LibraryName && prt.Name == prt.Name);
                    var idx = 0;
                    foreach(var part in parts)
                    {
                        bldr.Append($"G00 Z{job.SafeHeight}\n");
                        bldr.Append(feeder.CurrentPartGCode(row.RowNumber));
                        bldr.Append($"T0\n"); /* Set Place tool as current Z axis */
                        feeder.AdvancePart(row.RowNumber);
                        bldr.Append($"G00 Z{feeder.Feeder.PartZ}\n");
                        bldr.Append($"M63 P1\n"); /* Turn on Suction */
                        bldr.Append($"G04 P1\n"); /* Wait one second to capture part */
                        bldr.Append($"G00 Z{job.SafeHeight}\n");
                        bldr.Append($"T2\n"); /* Change tool to C (rotation) axis */
                        bldr.Append($"G00 X{machine.Settings.PartInspectionCamera.AbsolutePosition.X.ToDim()} Y{machine.Settings.PartInspectionCamera.AbsolutePosition.Y.ToDim()}\n");
                        bldr.Append($"M70 P1\n"); /* Send a message to PC to do part inspection */
                        bldr.Append($"G0 Z{part.RotateAngle}\n");
                        bldr.Append($"M80\n"); /* Pause job until machine verifies part in place */
                        bldr.Append($"G01 X{part.X.Value.ToDim()} Y{part.Y.Value.ToDim()}");
                        bldr.Append($"T0\n"); /* Set Place tool as current Z axis */
                        bldr.Append($"G00 Z{job.BoardZ} + {partType.Height}"); /* Place part on board */
                        bldr.Append($"M63 P0\n"); /* Turn off suction */
                        bldr.Append($"M64 P1\n"); /* Open exhaust to drop part */
                        bldr.Append($"G04 P1\n"); /* Wait one second */
                        bldr.Append($"G00 Z{job.SafeHeight} F300\n");
                        bldr.Append($"M64 P0\n"); /* Close exhaust  */
                    }
                }
            }


            bldr.Append($"M62 P0"); /* Turn off vacuum */

            return bldr.ToString();
        }
    }
}
