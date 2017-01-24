﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenCNCPilot.Core.Util;
using OpenCNCPilot.Core.GCode.GCodeCommands;
using OpenCNCPilot.Core.Platform;

namespace OpenCNCPilot.Core.GCode
{
	public enum ParseDistanceMode
	{
		Absolute,
		Incremental
	}

	public enum ParseUnit
	{
		Metric,
		Imperial
	}

	public class ParserState
	{
		public Vector3 Position;
		public ArcPlane Plane;
		public double Feed;
		public ParseDistanceMode DistanceMode;
		public ParseDistanceMode ArcDistanceMode;
		public ParseUnit Unit;
		public int LastMotionMode;

		public ParserState()
		{
			Position = new Vector3();
			Plane = ArcPlane.XY;
			Feed = 100;
			DistanceMode = ParseDistanceMode.Absolute;
			ArcDistanceMode = ParseDistanceMode.Incremental;
			Unit = ParseUnit.Metric;
			LastMotionMode = -1;
		}
	}

	struct Word
	{
		public char Command;
		public double Parameter;
	}

	public class GCodeParser
	{
        IStorage _storage;
        ILogger _logger;

		public ParserState State { get; private set; }

        //TODO: Removed compiled options
		private Regex GCodeSplitter = new Regex(@"([A-Z])\s*(\-?\d+\.?\d*)");
		private double[] MotionCommands = new double[] { 0, 1, 2, 3 };
		private string ValidWords = "GMXYZIJKFR";
		public  List<Command> Commands;

		public void Reset()
		{
			State = new ParserState();
			Commands = new List<Command>();	//don't reuse, might be used elsewhere
		}

		 public GCodeParser(IStorage storage, ILogger logger)
		{
			Reset();

            _logger = logger;
            _storage = storage;
		}

		public  void ParseFile(string path)
		{
			Parse(_storage.ReadLines(path));
		}

		public  void Parse(IEnumerable<string> file)
		{
			int i = 1;

			var sw = System.Diagnostics.Stopwatch.StartNew();

			foreach(string linei in file)
			{
				string line = CleanupLine(linei, i);

				if (string.IsNullOrWhiteSpace(line))
					continue;

				Parse(line.ToUpper(), i);

				i++;
			}

			sw.Stop();

            _logger.WriteLine($"Parsing the GCode File took {sw.ElapsedMilliseconds} ms");
		}

		 string CleanupLine(string line, int lineNumber)
		{
			int commentIndex = line.IndexOf(';');

			if (commentIndex > -1)
				line = line.Remove(commentIndex);

			int start = -1;

			while ((start = line.IndexOf('(')) != -1)
			{
				int end = line.IndexOf(')');

				if (end < start)
					throw new ParseException("mismatched parentheses", lineNumber);

				line = line.Remove(start, end - start);
			}

			return line;
		}

		 void Parse(string line, int lineNumber)
		{
			MatchCollection matches = GCodeSplitter.Matches(line);

			List<Word> Words = new List<Word>(matches.Count);

			foreach (Match match in matches)
			{
				Words.Add(new Word() { Command = match.Groups[1].Value[0], Parameter = double.Parse(match.Groups[2].Value, Constants.DecimalParseFormat) });
			}

			for (int i = 0; i < Words.Count; i++)
			{
				if (!ValidWords.Contains(Words[i].Command.ToString()))
				{
					Words.RemoveAt(i);
					continue;
				}

				if (Words[i].Command != 'F')
					continue;

				State.Feed = Words[i].Parameter;
				Words.RemoveAt(i);
				continue;
			}

			while (Words.Count > 0)
			{
				if (Words.First().Command == 'M')
				{
					int param = (int)Words.First().Parameter;

					if (param != Words.First().Parameter || param < 0)
						throw new ParseException("MCode can only have integer parameters", lineNumber);

					Commands.Add(new MCode() { Code = param });

					Words.RemoveAt(0);
					continue;
				}

				if (Words.First().Command == 'G' && !MotionCommands.Contains(Words.First().Parameter))
				{
					#region UnitPlaneDistanceMode

					double param = Words.First().Parameter;

					if (param == 90)
					{
						State.DistanceMode = ParseDistanceMode.Absolute;
						Words.RemoveAt(0);
						continue;
					}
					if (param == 91)
					{
						State.DistanceMode = ParseDistanceMode.Incremental;
						Words.RemoveAt(0);
						continue;
					}
					if (param == 90.1)
					{
						State.ArcDistanceMode = ParseDistanceMode.Absolute;
						Words.RemoveAt(0);
						continue;
					}
					if (param == 91.1)
					{
						State.ArcDistanceMode = ParseDistanceMode.Incremental;
						Words.RemoveAt(0);
						continue;
					}
					if (param == 21)
					{
						State.Unit = ParseUnit.Metric;
						Words.RemoveAt(0);
						continue;
					}
					if (param == 20)
					{
						State.Unit = ParseUnit.Imperial;
						Words.RemoveAt(0);
						continue;
					}
					if (param == 17)
					{
						State.Plane = ArcPlane.XY;
						Words.RemoveAt(0);
						continue;
					}
					if (param == 18)
					{
						State.Plane = ArcPlane.ZX;
						Words.RemoveAt(0);
						continue;
					}
					if (param == 19)
					{
						State.Plane = ArcPlane.YZ;
						Words.RemoveAt(0);
						continue;
					}

					Words.RemoveAt(0);  //unsupported G-Command
					continue;
					#endregion
				}

				break;
			}

			if (Words.Count == 0)
				return;

			int MotionMode = State.LastMotionMode;

			if (Words.First().Command == 'G')
			{
				MotionMode = (int)Words.First().Parameter;
				State.LastMotionMode = MotionMode;
				Words.RemoveAt(0);
			}

			if (MotionMode < 0)
				throw new ParseException("No Motion Mode active", lineNumber);

			double UnitMultiplier = (State.Unit == ParseUnit.Metric) ? 1 : 25.4;

			Vector3 EndPos = State.Position;

			#region FindEndPos
			{
				int Incremental = (State.DistanceMode == ParseDistanceMode.Incremental) ? 1 : 0;

				for (int i = 0; i < Words.Count; i++)
				{
					if (Words[i].Command != 'X')
						continue;
					EndPos.X = Words[i].Parameter * UnitMultiplier + Incremental * EndPos.X;
					Words.RemoveAt(i);
					break;
				}

				for (int i = 0; i < Words.Count; i++)
				{
					if (Words[i].Command != 'Y')
						continue;
					EndPos.Y = Words[i].Parameter * UnitMultiplier + Incremental * EndPos.Y;
					Words.RemoveAt(i);
					break;
				}

				for (int i = 0; i < Words.Count; i++)
				{
					if (Words[i].Command != 'Z')
						continue;
					EndPos.Z = Words[i].Parameter * UnitMultiplier + Incremental * EndPos.Z;
					Words.RemoveAt(i);
					break;
				}
			}
			#endregion

			if (MotionMode <= 1)
			{
				if (Words.Count > 0)
					throw new ParseException("Motion Command must be last in line (unused Words in Block)", lineNumber);

				var motion = new GCodeLine();
				motion.Start = State.Position;
				motion.End = EndPos;
				motion.Feed = State.Feed;
				motion.Rapid = MotionMode == 0;

				Commands.Add(motion);
				State.Position = EndPos;
				return;
			}

			double U, V;

			bool IJKused = false;

			switch (State.Plane)
			{
				default:
					U = State.Position.X;
					V = State.Position.Y;
					break;
				case ArcPlane.YZ:
					U = State.Position.Y;
					V = State.Position.Z;
					break;
				case ArcPlane.ZX:
					U = State.Position.Z;
					V = State.Position.X;
					break;
			}

			#region FindIJK
			{
				int ArcIncremental = (State.ArcDistanceMode == ParseDistanceMode.Incremental) ? 1 : 0;

				for (int i = 0; i < Words.Count; i++)
				{
					if (Words[i].Command != 'I')
						continue;

					switch(State.Plane)
					{
						case ArcPlane.XY:
							U = Words[i].Parameter * UnitMultiplier + ArcIncremental * State.Position.X;
                            break;
						case ArcPlane.YZ:
							throw new ParseException("Current Plane is YZ, I word is invalid", lineNumber);
						case ArcPlane.ZX:
							V = Words[i].Parameter * UnitMultiplier + ArcIncremental * State.Position.X;
							break;
					}

					IJKused = true;
					Words.RemoveAt(i);
					break;
				}

				for (int i = 0; i < Words.Count; i++)
				{
					if (Words[i].Command != 'J')
						continue;

					switch (State.Plane)
					{
						case ArcPlane.XY:
							V = Words[i].Parameter * UnitMultiplier + ArcIncremental * State.Position.Y;
							break;
						case ArcPlane.YZ:
							U = Words[i].Parameter * UnitMultiplier + ArcIncremental * State.Position.Y;
							break;
						case ArcPlane.ZX:
							throw new ParseException("Current Plane is ZX, J word is invalid", lineNumber);
					}

					IJKused = true;
					Words.RemoveAt(i);
					break;
				}

				for (int i = 0; i < Words.Count; i++)
				{
					if (Words[i].Command != 'K')
						continue;

					switch (State.Plane)
					{
						case ArcPlane.XY:
							throw new ParseException("Current Plane is XY, K word is invalid", lineNumber);
						case ArcPlane.YZ:
							V = Words[i].Parameter * UnitMultiplier + ArcIncremental * State.Position.Z;
							break;
						case ArcPlane.ZX:
							U = Words[i].Parameter * UnitMultiplier + ArcIncremental * State.Position.Z;
							break;
					}

					IJKused = true;
					Words.RemoveAt(i);
					break;
				}
			}
			#endregion

			#region ResolveRadius
			for (int i = 0; i < Words.Count; i++)
			{
				if (Words[i].Command != 'R')
					continue;

				if (IJKused)
					throw new ParseException("Both IJK and R notation used", lineNumber);

				if (State.Position == EndPos)
					throw new ParseException("arcs in R-notation must have non-coincident start and end points", lineNumber);

				double Radius = Words[i].Parameter * UnitMultiplier;

				if (Radius == 0)
					throw new ParseException("Radius can't be zero", lineNumber);

				double A, B;

				switch (State.Plane)
				{
					default:
						A = EndPos.X;
						B = EndPos.Y;
						break;
					case ArcPlane.YZ:
						A = EndPos.Y;
						B = EndPos.Z;
						break;
					case ArcPlane.ZX:
						A = EndPos.Z;
						B = EndPos.X;
						break;
				}

				A -= U;		//(AB) = vector from start to end of arc along the axes of the current plane
				B -= V;

				double C = -B;	//(UV) = vector perpendicular to (AB)
				double D = A;

				{	//normalize perpendicular vector
					double perpLength = Math.Sqrt(C * C + D * D);
					C /= perpLength;
					D /= perpLength;
				}

				double PerpSquare = (Radius * Radius) - ((A * A + B * B) / 4);

				if (PerpSquare < 0)
					throw new ParseException("arc radius too small to reach both ends", lineNumber);

				double PerpLength = Math.Sqrt(PerpSquare);

				if (MotionMode == 3 ^ Radius < 0)
					PerpLength = -PerpLength;

				U += (A / 2) + C * (PerpLength);
				V += (B / 2) + (D * PerpLength);

				Words.RemoveAt(i);
				break;
			}
			#endregion

			Arc arc = new Arc();
			arc.Start = State.Position;
			arc.End = EndPos;
			arc.Feed = State.Feed;
			arc.Direction = (MotionMode == 2) ? ArcDirection.CW : ArcDirection.CCW;
			arc.U = U;
			arc.V = V;
			arc.Plane = State.Plane;

			Commands.Add(arc);
			State.Position = EndPos;
			return;
		}
	}
}
