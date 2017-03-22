﻿using LagoVista.Core.Models.Drawing;
using System;

namespace LagoVista.Core.GCode.Commands
{
	public abstract class GCodeCommand : LagoVista.Core.Models.ModelBase
	{
        public enum StatusTypes
        {
            Ready,
            Queued,
            Sent,
            Acknowledged,
            Internal,
        }

        private StatusTypes _status = StatusTypes.Ready;
        public StatusTypes Status
        {
            get { return _status; }
            set { Set(ref _status, value); }
        }

        public double SpindlePRM { get; set; }

        public double Feed { get; set; }

        public string Line { get; set; }
        public int LineNumber { get; set; }

        public abstract TimeSpan EstimatedRunTime { get; }

        public DateTime? StartTimeStamp { get; set; }

        public abstract Vector3 CurrentPosition { get; }

        public int MessageLength { get { return Line.Length + 1; } }

        public virtual void SetComment(string comment) { }

        public double PauseTime { get; set; }

        public string Command
        {
            get
            {
                var parts = Line.Split(' ');
                return parts[0];
            }
        }

        public abstract void ApplyOffset(double x, double y, double angle);
    }
}
