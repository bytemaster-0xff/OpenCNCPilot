using HelixToolkit.Wpf;
using LagoVista.Core.GCode.Commands;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Models;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Xml;

namespace LagoVista.GCode.Sender.Application.Presentation
{
    public class HeightMapServices
    {        
        public static  void GetModel(HeightMap map, MeshGeometryVisual3D mesh)
        {
            MeshBuilder mb = new MeshBuilder(false, true);

            double Hdelta = map.MaxHeight - map.MinHeight;

            for (int x = 0; x < map.SizeX - 1; x++)
            {
                for (int y = 0; y < map.SizeY - 1; y++)
                {
                    if (!map.Points[x, y].HasValue || !map.Points[x, y + 1].HasValue || !map.Points[x + 1, y].HasValue || !map.Points[x + 1, y + 1].HasValue)
                        continue;

                    mb.AddQuad(
                        new System.Windows.Media.Media3D.Point3D(map.Min.X + (x + 1) * map.Delta.X / (map.SizeX - 1), map.Min.Y + (y) * map.Delta.Y / (map.SizeY - 1), map.Points[x + 1, y].Value),
                        new System.Windows.Media.Media3D.Point3D(map.Min.X + (x + 1) * map.Delta.X / (map.SizeX - 1), map.Min.Y + (y + 1) * map.Delta.Y / (map.SizeY - 1), map.Points[x + 1, y + 1].Value),
                        new System.Windows.Media.Media3D.Point3D(map.Min.X + (x) * map.Delta.X / (map.SizeX - 1), map.Min.Y + (y + 1) * map.Delta.Y / (map.SizeY - 1), map.Points[x, y + 1].Value),
                        new System.Windows.Media.Media3D.Point3D(map.Min.X + (x) * map.Delta.X / (map.SizeX - 1), map.Min.Y + (y) * map.Delta.Y / (map.SizeY - 1), map.Points[x, y].Value),
                        new System.Windows.Point(0, Convert.ToInt32( (map.Points[x + 1, y].Value - map.MinHeight) * Hdelta)),
                        new System.Windows.Point(0, Convert.ToInt32((map.Points[x + 1, y + 1].Value - map.MinHeight) * Hdelta)),
                        new System.Windows.Point(0, Convert.ToInt32((map.Points[x, y + 1].Value - map.MinHeight) * Hdelta)),
                        new System.Windows.Point(0, Convert.ToInt32((map.Points[x, y].Value - map.MinHeight) * Hdelta))
                        );
                }
            }

            mesh.MeshGeometry = mb.ToMesh();
        }

        public static void GetPreviewModel(HeightMap map, LinesVisual3D border, PointsVisual3D pointv)
        {
            GetPreviewModel(map.Min, map.Max, map.SizeX, map.SizeY, border, pointv);
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

        public static void GetModel(IEnumerable<GCodeCommand> toolPath, Settings settings, LinesVisual3D line, LinesVisual3D rapid, LinesVisual3D arc)
        {  
            Point3DCollection linePoints = new Point3DCollection();
            Point3DCollection rapidPoints = new Point3DCollection();
            Point3DCollection arcPoints = new Point3DCollection();

            foreach (GCodeCommand c in toolPath)
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

                var a = c as GCodeArc;

                if (a != null)
                {
                    foreach (GCodeMotion sub in a.Split(settings.ViewportArcSplit))
                    {
                        arcPoints.Add(sub.Start.ToPoint3D().ToMedia3D());
                        arcPoints.Add(sub.End.ToPoint3D().ToMedia3D());
                    }
                }
            }

            line.Points = linePoints;
            rapid.Points = rapidPoints;
            arc.Points = arcPoints;
        }
    }
}
