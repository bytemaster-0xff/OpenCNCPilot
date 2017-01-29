using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;

namespace LagoVista.Core.GCode.Commands
{
    public class GCodeLine : Motion
	{
		public bool Rapid;


		public override double Length
		{
			get
			{
				return Delta.Magnitude;
			}
		}

		public override Vector3 Interpolate(double ratio)
		{
			return Start + Delta * ratio;
		}

		public override IEnumerable<Motion> Split(double length)
		{
			int divisions = (int)Math.Ceiling(Length / length);

			if (divisions < 1)
				divisions = 1;

			Vector3 lastEnd = Start;

			for (int i = 1; i <= divisions; i++)
			{
				Vector3 end = Interpolate(((double)i) / divisions);

				var immediate = new GCodeLine();
				immediate.Start = lastEnd;
				immediate.End = end;
				immediate.Feed = Feed;

				yield return immediate;

				lastEnd = end;
			}
		}
	}
}
