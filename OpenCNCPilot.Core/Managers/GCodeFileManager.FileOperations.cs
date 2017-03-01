using LagoVista.Core.GCode;
using LagoVista.Core.GCode.Commands;
using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class GCodeFileManager
    {
        public Task OpenFileAsync(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                ClearPaths();
                FileName = "<empty>";
            }

            var parts = path.Split('\\');
            FileName = parts[parts.Length - 1];

            File = GCodeFile.Load(path);
            if (File != null)
            {
                FindExtents();
                RenderPaths();
            }
            else
            {
                Max = null;
                Min = null;
                ClearPaths();
            }

            return Task.FromResult(default(object));
        }

        private void FindExtents()
        {
            var min = new Point3D<double>() { X = 99999.0, Y = 99999.0, Z = 99999.0 };
            var max = new Point3D<double>() { X = -99999.0, Y = -99999.0, Z = -999999.0 };

            bool first = true;
            foreach (var cmd in File.Commands)
            {
                var motionCmd = cmd as GCodeMotion;
                if (motionCmd != null)
                {
                    if (!first)
                    {
                        min.X = Math.Min(min.X, motionCmd.Start.X);
                        min.Y = Math.Min(min.Y, motionCmd.Start.Y);
                        min.Z = Math.Min(min.Z, motionCmd.Start.Z);
                        min.X = Math.Min(min.X, motionCmd.End.X);
                        min.Y = Math.Min(min.Y, motionCmd.End.Y);
                        min.Z = Math.Min(min.Z, motionCmd.End.Z);
                    }
                    else
                    {
                        first = false;
                    }

                    max.X = Math.Max(max.X, motionCmd.Start.X);
                    max.Y = Math.Max(max.Y, motionCmd.Start.Y);
                    max.Z = Math.Max(max.Z, motionCmd.Start.Z);
                    max.X = Math.Max(max.X, motionCmd.End.X);
                    max.Y = Math.Max(max.Y, motionCmd.End.Y);
                    max.Z = Math.Max(max.Z, motionCmd.End.Z);
                }
            }

            Max = max;
            Min = min;
        }

        public Task CloseFileAsync()
        {
            throw new NotImplementedException();
        }
    }
}
