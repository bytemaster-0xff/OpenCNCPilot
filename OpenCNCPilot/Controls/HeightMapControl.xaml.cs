using System.Windows.Controls;
using System.Windows;
using System.Linq;
using LagoVista.GCode.Sender.ViewModels;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using LagoVista.EaglePCB.Managers;
using HelixToolkit.Wpf;
using System.Windows.Media;

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
            var linePoints = new Point3DCollection();

            var doc = XDocument.Load("./KegeratorController.brd");
            var pcb = EagleParser.ReadPCB(doc);

            var modelGroup = new Model3DGroup();
            var silverMaterial = MaterialHelper.CreateMaterial(Colors.Silver);
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
                        padMeshBuilder.AddBox(new Point3D(pad.X, pad.Y, 1), pad.DX, pad.DY, 1);
                        modelGroup.Children.Add(new GeometryModel3D() { Geometry = padMeshBuilder.ToMesh(true), Material = silverMaterial });
                    }
                }

                /*
                foreach (var circle in element.Package.Circles)
                {
                    var circleMeshBuilder = new MeshBuilder(false, false);
                    circleMeshBuilder.AddCylinder(new Point3D(circle.X, circle.Y, 0), new Point3D(circle.X, circle.Y, 3), circle.Radius);
                    modelGroup.Children.Add(new GeometryModel3D() { Geometry = circleMeshBuilder.ToMesh(true), Material = blackMaterial });
                }

                foreach (var circle in element.Package.Holes)
                {
                    var circleMeshBuilder = new MeshBuilder(false, false);
                    circleMeshBuilder.AddCylinder(new Point3D(circle.X, circle.Y, 0), new Point3D(circle.X, circle.Y, 3), circle.Drill);
                    modelGroup.Children.Add(new GeometryModel3D() { Geometry = circleMeshBuilder.ToMesh(true), Material = blackMaterial });
                }*/

                var boardEdgeMeshBuilder = new MeshBuilder(false, false);
                var outlineWires = pcb.Layers.Where(layer => layer.Number == 20).FirstOrDefault().Wires;

                boardEdgeMeshBuilder.AddBox(new Point3D(50, 40, 0), 100, 68, 1);
                boardEdgeMeshBuilder.AddBox(new Point3D(50, 3, 0), 88, 6, 1);
                boardEdgeMeshBuilder.AddBox(new Point3D(50, 77, 0), 88, 6, 1);

                boardEdgeMeshBuilder.AddCylinder(new Point3D(6, 6, -0.5),new Point3D(6,6,0.5),6, 50, true, true);
                boardEdgeMeshBuilder.AddCylinder(new Point3D(6, 74, -0.5), new Point3D(6, 74, 0.5), 6, 50, true, true);
                boardEdgeMeshBuilder.AddCylinder(new Point3D(94, 6, -0.5), new Point3D(94, 6, 0.5), 6, 50, true, true);
                boardEdgeMeshBuilder.AddCylinder(new Point3D(94, 74, -0.5), new Point3D(94, 74, 0.5), 6, 50, true, true);
                modelGroup.Children.Add(new GeometryModel3D() { Geometry = boardEdgeMeshBuilder.ToMesh(true), Material = greenMaterial });

            }

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
            Perspective,
            Flat,
        }

        ImageModes _imageMode = ImageModes.Perspective;

        private void ChangeView_Handler(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch (btn.Tag.ToString())
            {
                case "Top":
                    //  Camera.Position = new Point3D(ViewModel.Machine.Settings.WorkAreaWidth / 2, ViewModel.Machine.Settings.WorkAreaHeight / 2, 3000);
                    Camera.Position = new Point3D(50, 40, 200);
                    Camera.LookDirection = new Vector3D(0, 0.0001, -1);
                    _imageMode = ImageModes.Flat;
                    break;
                case "Left":
                    Camera.Position = new Point3D(-100, 40, 40);
                    Camera.LookDirection = new Vector3D(4, 0.0001, -1);
                    _imageMode = ImageModes.Flat;
                    break;
                case "Front":
                    Camera.Position = new Point3D(50, -120, 70);
                    Camera.LookDirection = new Vector3D(0, 4, -1.7);
                    _imageMode = ImageModes.Flat;
                    break;
                case "ZoomIn":
                    Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z - CAMERA_MOVE_DELTA);
                    break;
                case "ZoomOut":
                    Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z + CAMERA_MOVE_DELTA);
                    break;
                case "ShowObject":
                    break;
                case "ShowAll":
                    break;
                case "Up":
                    Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z + CAMERA_MOVE_DELTA);
                    break;
                case "UpLeft":
                    Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z + CAMERA_MOVE_DELTA);
                    break;
                case "UpRight":
                    Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z + CAMERA_MOVE_DELTA);
                    break;
                case "MoveLeft":
                    Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z);
                    break;
                case "Center":
                    break;
                case "Right":
                    Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z);
                    break;
                case "Down":
                    Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z - CAMERA_MOVE_DELTA);
                    break;
                case "DownRight":
                    Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z - CAMERA_MOVE_DELTA);
                    break;
                case "DownLeft":
                    Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z - CAMERA_MOVE_DELTA);
                    break;
                case "Forwards":
                    Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z);
                    break;
                case "Backwards":
                    Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z);
                    break;
            }
        }
    }
}
