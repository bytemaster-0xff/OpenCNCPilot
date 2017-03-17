using LagoVista.Core.GCode.Commands;
using LagoVista.GCode.Sender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class GCodeFileManager
    {
        public void ResetJob()
        {
            Head = 0;
            Tail = 0;
            _pendingToolChangeLine = null;

            if (Commands != null)
            {
                foreach (var cmd in Commands)
                {
                    cmd.Status = GCodeCommand.StatusTypes.Ready;
                }
            }
        }

        public void QueueAllItems()
        {
            foreach (var cmd in Commands)
            {
                cmd.Status = GCodeCommand.StatusTypes.Ready;
            }
        }

        public void ApplyHeightMap(HeightMap map)
        {
            if (_file == null)
            {
                _logger.Log(Core.PlatformSupport.LogLevel.Error, "GCodeFileManager_ApplyHeightMap", "Attempt to apply height map to empty gcode file.");
                _machine.AddStatusMessage(StatusMessageTypes.Warning, "Attempt to apply height map to empty gcode file");
            }
            else
            {
                _file = map.ApplyHeightMap(_file);
                IsDirty = true;
                RaisePropertyChanged(nameof(File));
                RaisePropertyChanged(nameof(Lines));
                RaisePropertyChanged(nameof(Arcs));
                RaisePropertyChanged(nameof(RapidMoves));
                RenderPaths();
            }
        }

        public void ArcToLines(double length)
        {
            if (_file == null)
            {
                _logger.Log(Core.PlatformSupport.LogLevel.Error, "GCodeFileManager_ArcToLines", "Attempt to convert arc to lines with empty gcode file.");
                _machine.AddStatusMessage(StatusMessageTypes.Warning, "Attempt to convert arc to lines with an empty gcode file");
            }
            else
            {
                _file = _file.ArcsToLines(length);
                IsDirty = true;
            }
        }

        public void ApplyOffset(double xOffset, double yOffset)
        {
            var newToolPath = new List<Core.GCode.Commands.GCodeCommand>();

            foreach (var command in _file.Commands)
            {
                var motionCommand = command as GCodeMotion;
                if(motionCommand != null)
                { 
                    motionCommand.Start = new Core.Models.Drawing.Vector3(motionCommand.Start.X + xOffset, motionCommand.Start.Y + yOffset, motionCommand.Start.Z);
                    motionCommand.End = new Core.Models.Drawing.Vector3(motionCommand.End.X + xOffset, motionCommand.End.Y + yOffset, motionCommand.End.Z);
                }
            }

            IsDirty = true;
            RaisePropertyChanged(nameof(IsDirty));          
            RaisePropertyChanged(nameof(File));
            RenderPaths();
        }



        public void StartJob()
        {
            _machine.SetMode(OperatingMode.SendingGCodeFile);
        }

        public void CancelJob()
        {
            _pendingToolChangeLine = null;

        }

        public void PauseJob()
        {

        }
    }
}
