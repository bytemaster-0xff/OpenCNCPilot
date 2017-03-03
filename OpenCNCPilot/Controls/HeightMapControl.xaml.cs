using System.Windows.Controls;
using System.Windows;
using System.Linq;
using LagoVista.GCode.Sender.ViewModels;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using LagoVista.EaglePCB.Managers;
using HelixToolkit.Wpf;
using System.Windows.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using LagoVista.EaglePCB.Models;

namespace LagoVista.GCode.Sender.Application.Controls
{

    public partial class HeightMapControl : UserControl
    {
        public HeightMapControl()
        {
            InitializeComponent();

            bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!designTime)
            {
                Loaded += HeightMapControl_Loaded;
            }
        }

        private void HeightMapControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Machine.GCodeFileManager.PropertyChanged += GCodeFileManager_PropertyChanged;
            ViewModel.Machine.Settings.PropertyChanged += GCodeFileManager_PropertyChanged;
            ViewModel.Machine.PropertyChanged += GCodeFileManager_PropertyChanged;

            ViewModel.Machine.PCBManager.PropertyChanged += PCBManager_PropertyChanged;

            var x = ViewModel.Machine.Settings.WorkAreaWidth / 2;
            Camera.Position = new Point3D(x, Camera.Position.Y, Camera.Position.Z);
        }

        private void RenderBoard(PCB board)
        {
            var linePoints = new Point3DCollection();

            var modelGroup = new Model3DGroup();
            var silverMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(0xb8, 0x73, 0x33));
            var redMaterial = MaterialHelper.CreateMaterial(Colors.Red);
            var greenMaterial = MaterialHelper.CreateMaterial(Colors.Green);
            var blueMaterial = MaterialHelper.CreateMaterial(Colors.Blue);
            var blackMaterial = MaterialHelper.CreateMaterial(Colors.Black);

            foreach (var element in board.Components)
            {
                foreach (var pad in element.SMDPads)
                {
                    var padMeshBuilder = new MeshBuilder(false, false);
                    padMeshBuilder.AddBox(new Point3D(pad.X, pad.Y, 1), pad.DX, pad.DY, 0.25);
                    modelGroup.Children.Add(new GeometryModel3D() { Geometry = padMeshBuilder.ToMesh(true), Material = silverMaterial });
                }

                foreach (var pad in element.Pads)
                {
                    var padMeshBuilder = new MeshBuilder(false, false);
                    padMeshBuilder.AddBox(new Point3D(pad.X, pad.Y, 1), 1, 1, 0.25);
                    modelGroup.Children.Add(new GeometryModel3D() { Geometry = padMeshBuilder.ToMesh(true), Material = silverMaterial });
                }
            }

            BottomWires.Points.Clear();
            TopWires.Points.Clear();

            foreach (var wire in board.TopWires)
            {
                TopWires.Points.Add(new Point3D(wire.Rect.X1, wire.Rect.Y1, 1));
                TopWires.Points.Add(new Point3D(wire.Rect.X2, wire.Rect.Y2, 1));
            }

            foreach (var wire in board.BottomWires)
            {
                BottomWires.Points.Add(new Point3D(wire.Rect.X1, wire.Rect.Y1, 1));
                BottomWires.Points.Add(new Point3D(wire.Rect.X2, wire.Rect.Y2, 1));
            }

            /*
            foreach (var circle in element.Package.Circles)
            {
                var circleMeshBuilder = new MeshBuilder(false, false);
                circleMeshBuilder.AddCylinder(new Point3D(circle.X, circle.Y, 0), new Point3D(circle.X, circle.Y, 3), circle.Radius);
                modelGroup.Children.Add(new GeometryModel3D() { Geometry = circleMeshBuilder.ToMesh(true), Material = blackMaterial });
            }
            */
            foreach (var circle in board.Holes)
            {
                var circleMeshBuilder = new MeshBuilder(false, false);
                circleMeshBuilder.AddCylinder(new Point3D(circle.X, circle.Y, -0.5), new Point3D(circle.X, circle.Y, 0.51), circle.Drill / 2);
                modelGroup.Children.Add(new GeometryModel3D() { Geometry = circleMeshBuilder.ToMesh(true), Material = blackMaterial });

                Debug.WriteLine(circle.X + " " + circle.Y); ;
            }

            var boardEdgeMeshBuilder = new MeshBuilder(false, false);

            #region Hold your nose to discover why irregular boards don't render as expected... 
            /* gonna cheat here in next chunk of code...need to make progress, assume all corners are
             * either square or round.  If rounded, same radius...WILL revisit this at some point, KDW 2/24/2017
             * FWIW - feel so dirty doing this, but need to move on :*( 
             * very happy to accept a PR to fix this!  Proper mechanism is to create a polygon and likely subdivide the curve into smaller polygon edges
             * more work than it's worth right now....sorry again :(
             */
            //TODO: Render proper edge of board.


            var cornerWires = board.Layers.Where(layer => layer.Number == 20).FirstOrDefault().Wires.Where(wire => wire.Curve.HasValue == true);
            var radius = cornerWires.Any() ? Math.Abs(cornerWires.First().Rect.X1 - cornerWires.First().Rect.X2) : 0;
            if (radius == 0)
            {
                boardEdgeMeshBuilder.AddBox(new Point3D(board.Width / 2, board.Height / 2, 0), board.Width, board.Height, 1);
            }
            else
            {
                boardEdgeMeshBuilder.AddBox(new Point3D(board.Width / 2, board.Height / 2, 0), board.Width - (radius * 2), board.Height - (radius * 2), 1);
                boardEdgeMeshBuilder.AddBox(new Point3D(board.Width / 2, radius / 2, 0), board.Width - (radius * 2), radius, 1);
                boardEdgeMeshBuilder.AddBox(new Point3D(board.Width / 2, board.Height - radius / 2, 0), board.Width - (radius * 2), radius, 1);
                boardEdgeMeshBuilder.AddBox(new Point3D(radius / 2, board.Height / 2, 0), radius, board.Height - (radius * 2), 1);
                boardEdgeMeshBuilder.AddBox(new Point3D(board.Width - radius / 2, board.Height / 2, 0), radius, board.Height - (radius * 2), 1);
                boardEdgeMeshBuilder.AddCylinder(new Point3D(radius, radius, -0.5), new Point3D(radius, radius, 0.5), radius, 50, true, true);
                boardEdgeMeshBuilder.AddCylinder(new Point3D(radius, board.Height - radius, -0.5), new Point3D(radius, board.Height - radius, 0.5), radius, 50, true, true);
                boardEdgeMeshBuilder.AddCylinder(new Point3D(board.Width - radius, radius, -0.5), new Point3D(board.Width - radius, radius, 0.5), radius, 50, true, true);
                boardEdgeMeshBuilder.AddCylinder(new Point3D(board.Width - radius, board.Height - radius, -0.5), new Point3D(board.Width - radius, board.Height - radius, 0.50), radius, 50, true, true);
            }
            #endregion

            modelGroup.Children.Add(new GeometryModel3D() { Geometry = boardEdgeMeshBuilder.ToMesh(true), Material = greenMaterial });

            PadsLayer.Content = modelGroup;

            RefreshExtents();
        }

        private void PCBManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.Machine.PCBManager.HasBoard))
            {
                if (ViewModel.Machine.PCBManager.HasBoard)
                    RenderBoard(ViewModel.Machine.PCBManager.Board);
            }
            else
            {
                PadsLayer.Content = null;
            }
        }

        private void GCodeFileManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.Machine.GCodeFileManager.Min) ||
                e.PropertyName == nameof(ViewModel.Machine.GCodeFileManager.Max) ||
                e.PropertyName == nameof(ViewModel.Machine.Settings.WorkAreaWidth) ||
                e.PropertyName == nameof(ViewModel.Machine.Settings.WorkAreaHeight) ||
                e.PropertyName == nameof(ViewModel.Machine.Settings))
            {
                RefreshExtents();
            }
        }

        private void RefreshExtents()
        {
            switch (_imageMode)
            {
                case ImageModes.Front: ShowFrontView(); break;
                case ImageModes.Side: ShowLeftView(); break;
                case ImageModes.Top: ShowTopView(); break;
            }
        }

        public void Clear()
        {
        }

        public MainViewModel ViewModel
        {
            get { return DataContext as MainViewModel; }
        }

        public bool ModelToolVisible
        {
            get { return ModelTool.Visible; }
            set { ModelTool.Visible = value; }
        }

        const int CAMERA_MOVE_DELTA = 10;

        public enum ImageModes
        {
            Top,
            Side,
            Front,
        }

        private void ShowLeftView()
        {
            double min = ViewModel.Machine.GCodeFileManager.HasValidFile ? ViewModel.Machine.GCodeFileManager.Min.Y : 0;
            double max = ViewModel.Machine.Settings.WorkAreaHeight;

            if (ViewModel.Machine.PCBManager.HasBoard)
                max = ViewModel.Machine.PCBManager.Board.Height;

            if (ViewModel.Machine.GCodeFileManager.HasValidFile)
                max = ViewModel.Machine.GCodeFileManager.Max.Y;

            var factor = 18.0;

            var delta = (max - min);
            var y = (delta / 2) + min;
            var x = -12 * factor;
            var z = 7 * factor;

            Camera.Position = new Point3D(x, y, z);
            Camera.LookDirection = new Vector3D(4, 0.0001, -1.7);
        }

        private void ShowTopView()
        {
            double minX = ViewModel.Machine.GCodeFileManager.HasValidFile ? ViewModel.Machine.GCodeFileManager.Min.X : 0;
            double maxX = ViewModel.Machine.Settings.WorkAreaWidth;

            if (ViewModel.Machine.PCBManager.HasBoard)
                maxX = ViewModel.Machine.PCBManager.Board.Width;

            if (ViewModel.Machine.GCodeFileManager.HasValidFile)
                maxX = ViewModel.Machine.GCodeFileManager.Max.X;

            double minY = ViewModel.Machine.GCodeFileManager.HasValidFile ? ViewModel.Machine.GCodeFileManager.Min.Y : 0;
            double maxY = ViewModel.Machine.Settings.WorkAreaHeight;

            if (ViewModel.Machine.PCBManager.HasBoard)
                maxY = ViewModel.Machine.PCBManager.Board.Height;

            if (ViewModel.Machine.GCodeFileManager.HasValidFile)
                maxY = ViewModel.Machine.GCodeFileManager.Max.Y;


            var deltaX = maxX - minX;
            var deltaY = maxY - maxY;

            var x = deltaX / 2 + minX;
            var y = deltaY / 2 + minY;

            Camera.Position = new Point3D(x, y, 400);
            Camera.LookDirection = new Vector3D(0, 0.0001, -1);

        }

        private void ShowFrontView()
        {
            double min = ViewModel.Machine.GCodeFileManager.HasValidFile ? ViewModel.Machine.GCodeFileManager.Min.X : 0;
            double max = ViewModel.Machine.Settings.WorkAreaWidth;

            if (ViewModel.Machine.PCBManager.HasBoard)
                max = ViewModel.Machine.PCBManager.Board.Width;

            if (ViewModel.Machine.GCodeFileManager.HasValidFile)
                max = ViewModel.Machine.GCodeFileManager.Max.X;

            var factor = 18.0;

            var delta = (max - min);
            var x = (delta / 2) + min;
            var y = -12 * factor;
            var z = 7 * factor;

            Camera.Position = new Point3D(x, y, z);
            Camera.LookDirection = new Vector3D(0.0001, 4, -1.7);
        }


        ImageModes _imageMode = ImageModes.Front;

        private void ChangeView_Handler(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch (btn.Tag.ToString())
            {
                case "Top":
                    {
                        _imageMode = ImageModes.Top;
                        ShowTopView();
                    }
                    break;
                case "Left":
                    {
                        _imageMode = ImageModes.Side;
                        ShowLeftView();
                    }
                    break;
                case "Front":
                    {
                        _imageMode = ImageModes.Front;
                        ShowFrontView();
                    }
                    break;
                case "ZoomIn":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y * 0.9, Camera.Position.Z * 0.9); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X * 0.9, Camera.Position.Y, Camera.Position.Z * 0.9); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z * .9); break;
                    }


                    Debug.WriteLine("ZI: " + Camera.Position.X + " " + Camera.Position.Y + " " + Camera.Position.Z);
                    break;
                case "ZoomOut":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y * 1.1, Camera.Position.Z * 1.1); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X * 1.1, Camera.Position.Y, Camera.Position.Z * 1.1); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z * 1.1); break;
                    }

                    Debug.WriteLine("ZO: " + Camera.Position.X + " " + Camera.Position.Y + " " + Camera.Position.Z);
                    break;
                case "ShowObject":
                    break;
                case "ShowAll":
                    break;
                case "Up":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }
                    break;
                case "UpLeft":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }
                    break;
                case "UpRight":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }
                    break;
                case "MoveLeft":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                    }
                    break;
                case "Center":
                    break;
                case "Right":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                    }
                    break;
                case "Down":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }
                    break;
                case "DownRight":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }
                    break;
                case "DownLeft":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }

                    break;
                case "Forwards":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                    }

                    break;
                case "Backwards":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                    }

                    break;
            }
        }
    }
}
