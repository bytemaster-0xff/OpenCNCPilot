using LagoVista.GCode.Sender.Models;
using System.Windows.Controls;
using System.Windows;
using System.Linq;
using LagoVista.GCode.Sender.ViewModels;
using LagoVista.GCode.Sender.Interfaces;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using LagoVista.EaglePCB.Managers;
using HelixToolkit.Wpf;
using System.Windows.Media;

namespace LagoVista.GCode.Sender.Application.Controls
{
    /// <summary>
    /// Interaction logic for HeightMapRenderer.xaml
    /// </summary>
    public partial class HeightMapControl : UserControl
    {
        public HeightMapControl()
        {
            InitializeComponent();

            Loaded += HeightMapControl_Loaded;
        }

        private void HeightMapControl_Loaded(object sender, RoutedEventArgs e)
        {
            Point3DCollection linePoints = new Point3DCollection();

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
                        meshBuilder.AddBox(new Point3D(pad.X, pad.Y, 5),pad.DX, pad.DY,0.25);
                        modelGroup.Children.Add(new GeometryModel3D() { Geometry = meshBuilder.ToMesh(true), Material = redMaterial });
                    }
                }
            }

            PadsLayer.Content = modelGroup;
        }

        public void Clear()
        {
            ModelHeightMap.MeshGeometry = new System.Windows.Media.Media3D.MeshGeometry3D();
            ModelHeightMapBoundary.Points.Clear();
            ModelHeightMapPoints.Points.Clear();
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


        public static readonly DependencyProperty HeightMapProperty
            = DependencyProperty.Register("HeightMap", typeof(HeightMap), typeof(HeightMapControl), new PropertyMetadata(HeightMapChangedCallback));

        public static void HeightMapChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = obj as HeightMapControl;
            ctl.HeightMap = args.NewValue as HeightMap;
        }


        public HeightMap HeightMap
        {
            get { return GetValue(HeightMapProperty) as HeightMap; }
            set
            {
                SetValue(HeightMapProperty, value);
                if (value == null)
                {
                    Clear();
                }
                else
                {
                    Presentation.HeightMapServices.GetPreviewModel(value, ModelHeightMapBoundary, ModelHeightMapPoints);
                }
            }
        }
    }
}
