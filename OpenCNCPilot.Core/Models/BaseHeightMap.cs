using System.Xml;
using System;
using System.Collections.Generic;
using LagoVista.Core.GCode;
using LagoVista.Core.GCode.Commands;
using LagoVista.Core.Models.Drawing;

namespace LagoVista.GCode.Sender.Models
{
	public abstract class BaseHeightMap
	{
        private Settings _settings;
		public double?[,] Points { get; private set; }
		public int SizeX { get; private set; }
		public int SizeY { get; private set; }

		public int Progress { get { return TotalPoints - NotProbed.Count; } }
		public int TotalPoints { get { return SizeX * SizeY; } }

		public Queue<Tuple<int, int>> NotProbed { get; private set; } = new Queue<Tuple<int, int>>();
       public Vector2 Min { get; private set; }
		public Vector2 Max { get; private set; }

		public Vector2 Delta { get { return Max - Min; } }

		public double MinHeight { get; private set; } = 0;
		public double MaxHeight { get; private set; } = 0;

		public event Action MapUpdated;

		public double GridX { get { return (Max.X - Min.X) / (SizeX - 1); } }
		public double GridY { get { return (Max.Y - Min.Y) / (SizeY - 1); } }


		public BaseHeightMap(Settings settings, double gridSize, Vector2 min, Vector2 max)
		{
            _settings = settings;

			if (min.X == max.X || min.Y == max.Y)
				throw new Exception("Height map can't be infinitely narrow");

			int pointsX = (int)Math.Ceiling((max.X - min.X) / gridSize) + 1;
			int pointsY = (int)Math.Ceiling((max.Y - min.Y) / gridSize) + 1;

			if (pointsX == 0 || pointsY == 0)
				throw new Exception("Height map must have at least 4 points");

			Points = new double?[pointsX, pointsY];

			if (max.X < min.X)
			{
				double a = min.X;
				min.X = max.X;
				max.X = a;
			}

			if (max.Y < min.Y)
			{
				double a = min.Y;
				min.Y = max.Y;
				max.Y = a;
			}

			Min = min;
			Max = max;

			SizeX = pointsX;
			SizeY = pointsY;

			for (int x = 0; x < SizeX; x++)
			{
				for (int y = 0; y < SizeY; y++)
					NotProbed.Enqueue(new Tuple<int, int>(x, y));

				if (++x >= SizeX)
					break;

				for (int y = SizeY - 1; y >= 0; y--)
					NotProbed.Enqueue(new Tuple<int, int>(x, y));
			}
		}

        public BaseHeightMap(Settings settings)
        {
            _settings = settings;
        }

		public double InterpolateZ(double x, double y)
		{
			if (x > Max.X || x < Min.X || y > Max.Y || y < Min.Y)
				return MaxHeight;

			x -= Min.X;
			y -= Min.Y;

			x /= GridX;
			y /= GridY;

			int iLX = (int)Math.Floor(x);   //lower integer part
			int iLY = (int)Math.Floor(y);

			int iHX = (int)Math.Ceiling(x); //upper integer part
			int iHY = (int)Math.Ceiling(y);

			double fX = x - iLX;             //fractional part
			double fY = y - iLY;

			double linUpper = Points[iHX, iHY].Value * fX + Points[iLX, iHY].Value * (1 - fX);       //linear immediates
			double linLower = Points[iHX, iLY].Value * fX + Points[iLX, iLY].Value * (1 - fX);

			return linUpper * fY + linLower * (1 - fY);     //bilinear result
		}

		public Vector2 GetCoordinates(int x, int y)
		{
			return new Vector2(x * (Delta.X / (SizeX - 1)) + Min.X, y * (Delta.Y / (SizeY - 1)) + Min.Y);
		}

        public void FillWithTestPattern(string pattern)
        {
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    double X = (x * (Max.X - Min.X)) / (SizeX - 1) + Min.X;
                    double Y = (y * (Max.Y - Min.Y)) / (SizeY - 1) + Min.Y;

                    var d = (X * X + Y * Y) / 1000.0;
                    AddPoint(x, y, (double)d);
                }
            }
        }

        public void AddPoint(int x, int y, double height)
		{
			Points[x, y] = height;

			if (height > MaxHeight)
				MaxHeight = height;
			if (height < MinHeight)
				MinHeight = height;

			if (MapUpdated != null)
				MapUpdated();
		}

	
		public void SaveToStream(System.IO.Stream path)
		{
			XmlWriterSettings set = new XmlWriterSettings();
			set.Indent = true;
            using (XmlWriter w = XmlWriter.Create(path, set))
            {
                w.WriteStartDocument();
                w.WriteStartElement("heightmap");
                w.WriteAttributeString("MinX", Min.X.ToString(Constants.DecimalParseFormat));
                w.WriteAttributeString("MinY", Min.Y.ToString(Constants.DecimalParseFormat));
                w.WriteAttributeString("MaxX", Max.X.ToString(Constants.DecimalParseFormat));
                w.WriteAttributeString("MaxY", Max.Y.ToString(Constants.DecimalParseFormat));
                w.WriteAttributeString("SizeX", SizeX.ToString(Constants.DecimalParseFormat));
                w.WriteAttributeString("SizeY", SizeY.ToString(Constants.DecimalParseFormat));

                for (int x = 0; x < SizeX; x++)
                {
                    for (int y = 0; y < SizeY; y++)
                    {
                        if (!Points[x, y].HasValue)
                            continue;

                        w.WriteStartElement("point");
                        w.WriteAttributeString("X", x.ToString());
                        w.WriteAttributeString("Y", y.ToString());
                        w.WriteString(Points[x, y].Value.ToString(Constants.DecimalParseFormat));
                        w.WriteEndElement();
                    }
                }
                w.WriteEndElement();
            }
            
		}

        protected void Load(XmlReader r)
        {
            while (r.Read())
            {
                if (!r.IsStartElement())
                    continue;

                switch (r.Name)
                {
                    case "heightmap":
                        Min = new Vector2(double.Parse(r["MinX"], Constants.DecimalParseFormat), double.Parse(r["MinY"], Constants.DecimalParseFormat));
                        Max = new Vector2(double.Parse(r["MaxX"], Constants.DecimalParseFormat), double.Parse(r["MaxY"], Constants.DecimalParseFormat));
                        SizeX = int.Parse(r["SizeX"]);
                        SizeY = int.Parse(r["SizeY"]);
                        Points = new double?[SizeX, SizeY];
                        break;
                    case "point":
                        int x = int.Parse(r["X"]), y = int.Parse(r["Y"]);
                        double height = double.Parse(r.ReadInnerXml(), Constants.DecimalParseFormat);

                        Points[x, y] = height;

                        if (height > MaxHeight)
                            MaxHeight = height;
                        if (height < MinHeight)
                            MinHeight = height;

                        break;
                }
            }


            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                    if (!Points[x, y].HasValue)
                        NotProbed.Enqueue(new Tuple<int, int>(x, y));

                if (++x >= SizeX)
                    break;

                for (int y = SizeY - 1; y >= 0; y--)
                    if (!Points[x, y].HasValue)
                        NotProbed.Enqueue(new Tuple<int, int>(x, y));
            }
        }


        public GCodeFile ApplyHeightMap(GCodeFile file)
        {
            double segmentLength = Math.Min(GridX, GridY);

            var newToolPath = new List<Command>();

            foreach (var command in file.Commands)
            {
                if (command is MCode)
                {
                    newToolPath.Add(command);
                    continue;
                }
                else
                {
                    Motion m = (Motion)command;

                    foreach (Motion subMotion in m.Split(segmentLength))
                    {
                        subMotion.Start.Z += InterpolateZ(subMotion.Start.X, subMotion.Start.Y);
                        subMotion.End.Z += InterpolateZ(subMotion.End.X, subMotion.End.Y);

                        newToolPath.Add(subMotion);
                    }
                }
            }

            return GCodeFile.FromCommands(newToolPath);
        }

    }


}
