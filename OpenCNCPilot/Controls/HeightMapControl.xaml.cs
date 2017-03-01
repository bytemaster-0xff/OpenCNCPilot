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

            return;
            var linePoints = new Point3DCollection();

            var doc = XDocument.Load("./KegeratorController.brd");
            // var doc = XDocument.Load("./EagleSample.brd");
            var pcb = EagleParser.ReadPCB(doc);

            var modelGroup = new Model3DGroup();
            var silverMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(0xb8, 0x73, 0x33));
            var redMaterial = MaterialHelper.CreateMaterial(Colors.Red);
            var greenMaterial = MaterialHelper.CreateMaterial(Colors.Green);
            var blueMaterial = MaterialHelper.CreateMaterial(Colors.Blue);
            var blackMaterial = MaterialHelper.CreateMaterial(Colors.Black);

            foreach (var element in pcb.Components)
            {
                if (element.SMDPads.Any())
                {
                    foreach (var pad in element.SMDPads)
                    {
                        var padMeshBuilder = new MeshBuilder(false, false);
                        padMeshBuilder.AddBox(new Point3D(pad.X, pad.Y, 1), pad.DX, pad.DY, 0.25);
                        modelGroup.Children.Add(new GeometryModel3D() { Geometry = padMeshBuilder.ToMesh(true), Material = silverMaterial });
                    }
                }
            }
            /*
            foreach (var circle in element.Package.Circles)
            {
                var circleMeshBuilder = new MeshBuilder(false, false);
                circleMeshBuilder.AddCylinder(new Point3D(circle.X, circle.Y, 0), new Point3D(circle.X, circle.Y, 3), circle.Radius);
                modelGroup.Children.Add(new GeometryModel3D() { Geometry = circleMeshBuilder.ToMesh(true), Material = blackMaterial });
            }
            */
            foreach (var circle in pcb.Holes)
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


            var cornerWires = pcb.Layers.Where(layer => layer.Number == 20).FirstOrDefault().Wires.Where(wire => wire.Curve.HasValue == true);
            var radius = cornerWires.Any() ? cornerWires.First().Rect.X2 : 0;
            if (radius == 0)
            {
                boardEdgeMeshBuilder.AddBox(new Point3D(pcb.Width / 2, pcb.Height / 2, 0), pcb.Width, pcb.Height, 1);
            }
            else
            {
                boardEdgeMeshBuilder.AddBox(new Point3D(pcb.Width / 2, pcb.Height / 2, 0), pcb.Width - (radius * 2), pcb.Height - (radius * 2), 1);
                boardEdgeMeshBuilder.AddBox(new Point3D(pcb.Width / 2, radius / 2, 0), pcb.Width - (radius * 2), radius, 1);
                boardEdgeMeshBuilder.AddBox(new Point3D(pcb.Width / 2, pcb.Height - radius / 2, 0), pcb.Width - (radius * 2), radius, 1);
                boardEdgeMeshBuilder.AddBox(new Point3D(radius / 2, pcb.Height / 2, 0), radius, pcb.Height - (radius * 2), 1);
                boardEdgeMeshBuilder.AddBox(new Point3D(pcb.Width - radius / 2, pcb.Height / 2, 0), radius, pcb.Height - (radius * 2), 1);
                boardEdgeMeshBuilder.AddCylinder(new Point3D(radius, radius, -0.5), new Point3D(radius, radius, 0.5), radius, 50, true, true);
                boardEdgeMeshBuilder.AddCylinder(new Point3D(radius, pcb.Height - radius, -0.5), new Point3D(radius, pcb.Height - radius, 0.5), radius, 50, true, true);
                boardEdgeMeshBuilder.AddCylinder(new Point3D(pcb.Width - radius, radius, -0.5), new Point3D(pcb.Width - radius, radius, 0.5), radius, 50, true, true);
                boardEdgeMeshBuilder.AddCylinder(new Point3D(pcb.Width - radius, pcb.Height - radius, -0.5), new Point3D(pcb.Width - radius, pcb.Height - radius, 0.50), radius, 50, true, true);
            }
            #endregion

            modelGroup.Children.Add(new GeometryModel3D() { Geometry = boardEdgeMeshBuilder.ToMesh(true), Material = greenMaterial });



            PadsLayer.Content = modelGroup;
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

        ImageModes _imageMode = ImageModes.Front;

        private void ChangeView_Handler(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch (btn.Tag.ToString())
            {
                case "Top":
                    //  Camera.Position = new Point3D(ViewModel.Machine.Settings.WorkAreaWidth / 2, ViewModel.Machine.Settings.WorkAreaHeight / 2, 3000);
                    Camera.Position = new Point3D(50, 40, 200);
                    Camera.LookDirection = new Vector3D(0, 0.0001, -1);
                    _imageMode = ImageModes.Top;
                    break;
                case "Left":
                    Camera.Position = new Point3D(-100, 40, 40);
                    Camera.LookDirection = new Vector3D(4, 0.0001, -1);
                    _imageMode = ImageModes.Side;
                    break;
                case "Front":
                    Camera.Position = new Point3D(50, -120, 70);
                    Camera.LookDirection = new Vector3D(0, 4, -1.7);
                    _imageMode = ImageModes.Front;
                    break;
                case "ZoomIn":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y * 0.9, Camera.Position.Z * 0.9); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X * 0.9, Camera.Position.Y, Camera.Position.Z * 0.9); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X * 0.9, Camera.Position.Y * 0.9, Camera.Position.Z); break;
                    }
                    break;
                case "ZoomOut":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y * 1.1, Camera.Position.Z * 1.1); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X * 1.1, Camera.Position.Y, Camera.Position.Z * 1.1); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X * 1.1, Camera.Position.Y * 1.1, Camera.Position.Z); break;
                    }
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
