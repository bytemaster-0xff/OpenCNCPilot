using LagoVista.GCode.Sender.Models;
using System.Windows.Controls;
using System.Windows;
using LagoVista.GCode.Sender.ViewModels;

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
            this.Loaded += HeightMapControl_Loaded;
        }

        private void HeightMapControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Machine.PropertyChanged += Machine_PropertyChanged;
        }

        private void Machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {   if(e.PropertyName == "CurrentJob")
            {
                CurrentJob = ViewModel.Machine.CurrentJob;
            }
        }

        public void Clear()
        {
            ModelHeightMap.MeshGeometry = new System.Windows.Media.Media3D.MeshGeometry3D();
            ModelHeightMapBoundary.Points.Clear();
            ModelHeightMapPoints.Points.Clear();
            ModelLine.Points.Clear();
            ModelRapid.Points.Clear();
            ModelArc.Points.Clear();
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

        public static readonly DependencyProperty CurrentJobProperty =
                DependencyProperty.Register("CurrentJob", typeof(IJobProcessor),
                    typeof(HeightMapControl),
                    new PropertyMetadata(PropChangeCallback));

        private static void PropChangeCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = obj as HeightMapControl;
            ctl.CurrentJob = args.NewValue as IJobProcessor;
        }

        public IJobProcessor CurrentJob
        {
            get { return GetValue(CurrentJobProperty) as IJobProcessor; }
            set
            {
                SetValue(CurrentJobProperty, value);
                if (value != null)
                    Presentation.HeightMapServices.GetModel(value.Commands, ViewModel.Machine.Settings.ViewportArcSplit, ModelLine, ModelRapid, ModelArc);
                else
                    Clear();
            }
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
