using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;

namespace LagoVista.Core.GCode.Commands
{
    public abstract class Motion : Command
	{ 
		public Vector3 Start;
		public Vector3 End;
		public double Feed;
		public Vector3 Delta
		{
			get
			{
				return End - Start;
			}
		}

        public DateTime? StartTimeStamp { get; set; }

		/// <summary>
		/// Total travel distance of tool
		/// </summary>
		public abstract double Length { get; }

		/// <summary>
		/// get intermediate point along the path
		/// </summary>
		/// <param name="ratio">ratio between intermediate point and end</param>
		/// <returns>intermediate point</returns>
		public abstract Vector3 Interpolate(double ratio);

		/// <summary>
		/// Split motion into smaller fragments, still following the same path
		/// </summary>
		/// <param name="length">the maximum allowed length per returned segment</param>
		/// <returns>collection of smaller motions that together form this motion</returns>
		public abstract IEnumerable<Motion> Split(double length);

        public override TimeSpan EstimatedRunTime
        {
            get
            {
                if(Length == 0 || Feed == 0) 
                {
                    return TimeSpan.Zero;
                }

                /* Feed is units per minute, we care about seconds */
                var feedPerSeconds = Feed / 60;
                return TimeSpan.FromSeconds(Length / feedPerSeconds);
            }
        }

        public override string ToString()
        {
            return String.Format("{0}. - {1}    Duration:[{2:0}ms]    Length[{3:0.0}mm]   Feed[{4:0}]", LineNumber, Line.Trim('\r','\n'), EstimatedRunTime.TotalMilliseconds, Length, Feed);
        }

        public Vector3 CurrentPosition
        {
            get
            {
                if(StartTimeStamp.HasValue && EstimatedRunTime.TotalMilliseconds > 0)
                {
                    var ms = (DateTime.Now - StartTimeStamp.Value).TotalMilliseconds;
                    var percentComplete = ms / EstimatedRunTime.TotalMilliseconds;
                    return Interpolate(percentComplete);
                }
                else
                {
                    return Start;
                }
            }
        }
    }
}
