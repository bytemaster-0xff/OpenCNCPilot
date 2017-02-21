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
            var redMaterial = MaterialHelper.CreateMaterial(Colors.Red);
            foreach (var element in pcb.Components)
            {
                if (element.SMDPads.Any())
                {
                    foreach (var pad in element.SMDPads)
                    {
                        var meshBuilder = new MeshBuilder(false, false);
                        meshBuilder.AddBox(new Point3D(pad.X, pad.Y, 0), pad.DX, pad.DY, 1);
                        modelGroup.Children.Add(new GeometryModel3D() { Geometry = meshBuilder.ToMesh(true), Material = redMaterial });
                    }
                }
            }

            PadsLayer.Content = modelGroup;
            //viewport.ZoomExtents(1.5);
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
                case "Perspective":
                    Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z - CAMERA_MOVE_DELTA);
                    _imageMode = ImageModes.Perspective;
                    break;
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
                    Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z - CAMERA_MOVE_DELTA);
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
