using HelixToolkit.Wpf;
using OpenCNCPilot.Core;
using OpenCNCPilot.Core.GCode;
using OpenCNCPilot.Core.GCode.GCodeCommands;
using OpenCNCPilot.Core.Platform;
using OpenCNCPilot.Core.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Media.Media3D;
using System.Xml;

namespace OpenCNCPilot.Presentation
{
    public class HeightMap : BaseHeightMap
    {
        ILogger _logger;
        Settings _settings;

        public HeightMap(Settings settings, ILogger logger,  double gridSize, Vector2 min, Vector2 max) : base(gridSize,  min,  max)
        {
            _settings = settings;
            _logger = logger;
        }

        public HeightMap(Settings settings, ILogger logger) : base()
        {
            _settings = settings;
            _logger = logger;
        }


        public void GetModel(MeshGeometryVisual3D mesh)
        {
            MeshBuilder mb = new MeshBuilder(false, true);

            double Hdelta = MaxHeight - MinHeight;

            for (int x = 0; x < SizeX - 1; x++)
            {
                for (int y = 0; y < SizeY - 1; y++)
                {
                    if (!Points[x, y].HasValue || !Points[x, y + 1].HasValue || !Points[x + 1, y].HasValue || !Points[x + 1, y + 1].HasValue)
                        continue;

                    mb.AddQuad(
                        new System.Windows.Media.Media3D.Point3D(Min.X + (x + 1) * Delta.X / (SizeX - 1), Min.Y + (y) * Delta.Y / (SizeY - 1), Points[x + 1, y].Value),
                        new System.Windows.Media.Media3D.Point3D(Min.X + (x + 1) * Delta.X / (SizeX - 1), Min.Y + (y + 1) * Delta.Y / (SizeY - 1), Points[x + 1, y + 1].Value),
                        new System.Windows.Media.Media3D.Point3D(Min.X + (x) * Delta.X / (SizeX - 1), Min.Y + (y + 1) * Delta.Y / (SizeY - 1), Points[x, y + 1].Value),
                        new System.Windows.Media.Media3D.Point3D(Min.X + (x) * Delta.X / (SizeX - 1), Min.Y + (y) * Delta.Y / (SizeY - 1), Points[x, y].Value),
                        new System.Windows.Point(0, Convert.ToInt32( (Points[x + 1, y].Value - MinHeight) * Hdelta)),
                        new System.Windows.Point(0, Convert.ToInt32((Points[x + 1, y + 1].Value - MinHeight) * Hdelta)),
                        new System.Windows.Point(0, Convert.ToInt32((Points[x, y + 1].Value - MinHeight) * Hdelta)),
                        new System.Windows.Point(0, Convert.ToInt32((Points[x, y].Value - MinHeight) * Hdelta))
                        );
                }
            }

            mesh.MeshGeometry = mb.ToMesh();
        }

        public void GetPreviewModel(LinesVisual3D border, PointsVisual3D pointv)
        {
            GetPreviewModel(Min, Max, SizeX, SizeY, border, pointv);
        }

        public void FillWithTestPattern(string pattern)
        {
            DataTable t = new DataTable();

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    double X = (x * (Max.X - Min.X)) / (SizeX - 1) + Min.X;
                    double Y = (y * (Max.Y - Min.Y)) / (SizeY - 1) + Min.Y;

                    decimal d = (decimal)t.Compute(pattern.Replace("x", X.ToString()).Replace("y", Y.ToString()), "");
                    AddPoint(x, y, (double)d);
                }
            }
        }

        public static void GetPreviewModel(Vector2 min, Vector2 max, double gridSize, LinesVisual3D border, PointsVisual3D pointv)
        {
            Vector2 min_temp = new Vector2(Math.Min(min.X, max.X), Math.Min(min.Y, max.Y));
            Vector2 max_temp = new Vector2(Math.Max(min.X, max.X), Math.Max(min.Y, max.Y));

            min = min_temp;
            max = max_temp;

            if ((max.X - min.X) == 0 || (max.Y - min.Y) == 0)
            {
                pointv.Points.Clear();
                border.Points.Clear();
                return;
            }

            int pointsX = (int)Math.Ceiling((max.X - min.X) / gridSize) + 1;
            int pointsY = (int)Math.Ceiling((max.Y - min.Y) / gridSize) + 1;

            GetPreviewModel(min, max, pointsX, pointsY, border, pointv);
        }

        public static void GetPreviewModel(Vector2 min, Vector2 max, int pointsX, int pointsY, LinesVisual3D border, PointsVisual3D pointv)
        {
            Vector2 min_temp = new Vector2(Math.Min(min.X, max.X), Math.Min(min.Y, max.Y));
            Vector2 max_temp = new Vector2(Math.Max(min.X, max.X), Math.Max(min.Y, max.Y));

            min = min_temp;
            max = max_temp;

            double gridX = (max.X - min.X) / (pointsX - 1);
            double gridY = (max.Y - min.Y) / (pointsY - 1);

            Point3DCollection points = new Point3DCollection();

            for (int x = 0; x < pointsX; x++)
            {
                for (int y = 0; y < pointsY; y++)
                {
                    points.Add(new System.Windows.Media.Media3D.Point3D(min.X + x * gridX, min.Y + y * gridY, 0));
                }
            }

            pointv.Points.Clear();
            pointv.Points = points;

            Point3DCollection b = new Point3DCollection();
            b.Add(new System.Windows.Media.Media3D.Point3D(min.X, min.Y, 0));
            b.Add(new System.Windows.Media.Media3D.Point3D(min.X, max.Y, 0));
            b.Add(new System.Windows.Media.Media3D.Point3D(min.X, max.Y, 0));
            b.Add(new System.Windows.Media.Media3D.Point3D(max.X, max.Y, 0));
            b.Add(new System.Windows.Media.Media3D.Point3D(max.X, max.Y, 0));
            b.Add(new System.Windows.Media.Media3D.Point3D(max.X, min.Y, 0));
            b.Add(new System.Windows.Media.Media3D.Point3D(max.X, min.Y, 0));
            b.Add(new System.Windows.Media.Media3D.Point3D(min.X, min.Y, 0));

            border.Points.Clear();
            border.Points = b;
        }

        public static void GetModel(IEnumerable<Command> toolPath, Settings settings, ILogger logger, LinesVisual3D line, LinesVisual3D rapid, LinesVisual3D arc)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            Point3DCollection linePoints = new Point3DCollection();
            Point3DCollection rapidPoints = new Point3DCollection();
            Point3DCollection arcPoints = new Point3DCollection();

            foreach (Command c in toolPath)
            {
                var l = c as GCodeLine;

                if (l != null)
                {
                    if (l.Rapid)
                    {
                        rapidPoints.Add(l.Start.ToPoint3D().ToMedia3D());
                        rapidPoints.Add(l.End.ToPoint3D().ToMedia3D());
                    }
                    else
                    {
                        linePoints.Add(l.Start.ToPoint3D().ToMedia3D());
                        linePoints.Add(l.End.ToPoint3D().ToMedia3D());
                    }
                    continue;
                }

                var a = c as Arc;

                if (a != null)
                {
                    foreach (Motion sub in a.Split(settings.ViewportArcSplit))
                    {
                        arcPoints.Add(sub.Start.ToPoint3D().ToMedia3D());
                        arcPoints.Add(sub.End.ToPoint3D().ToMedia3D());
                    }
                }
            }

            line.Points = linePoints;
            rapid.Points = rapidPoints;
            arc.Points = arcPoints;

            sw.Stop();
            logger.WriteLine($"Generating the Toolpath Model took {sw.ElapsedMilliseconds} ms");
        }

        public static HeightMap Load(string path, Settings settings, ILogger logger)
        {
            var map = new HeightMap(settings, logger);

            var r = XmlReader.Create(path);
            map.Load(r);           

            return map;
        }
    }
}
