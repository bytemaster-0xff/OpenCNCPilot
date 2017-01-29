using LagoVista.Core.GCode.Commands;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Application.Presentation;
using LagoVista.GCode.Sender.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;


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


        private void HeightMapControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                App.Current.Machine.PropertyChanged += (sndr, args) =>
                {
                    if (args.PropertyName == nameof(App.Current.Machine.HasJob))
                    {
                        Presentation.HeightMapServices.GetModel(App.Current.Machine.CurrentJob.Commands, App.Current.Settings, ModelLine, ModelRapid, ModelArc);
                    }
                };
            }
        }

        public void SetPreviewModel(HeightMap map)
        {
            //map.GetPreviewModel(ModelHeightMapBoundary, ModelHeightMapPoints);
        }


        public void GetModel(HeightMap map)
        {
            //map.GetModel(ModelHeightMap);
        }

        public void Clear()
        {
            ModelHeightMap.MeshGeometry = new System.Windows.Media.Media3D.MeshGeometry3D();
            ModelHeightMapBoundary.Points.Clear();
            ModelHeightMapPoints.Points.Clear();
        }


        public void SetPreviewModel(List<GCodeCommand> commands)
        {

        }

        public void RefreshToolPosition()
        {
            ModelTool.Point1 = (App.Current.Machine.WorkPosition + new Vector3(0, 0, 10)).ToPoint3D().ToMedia3D();
            ModelTool.Point2 = App.Current.Machine.WorkPosition.ToPoint3D().ToMedia3D();
        }

        public bool ModelToolVisible
        {
            get { return ModelTool.Visible; }
            set { ModelTool.Visible = value; }
        }

        private HeightMap _heightMap;
        public HeightMap HeightMap
        {
            get { return _heightMap; }
            set
            {
                _heightMap = value;
                if (_heightMap == null)
                {
                    Clear();
                }
                else
                {
                    Presentation.HeightMapServices.GetPreviewModel(_heightMap, ModelHeightMapBoundary, ModelHeightMapPoints);
                }
            }
        }


        IMachine _machine;
        public IMachine Machine
        {
            get { return _machine; }
            set
            {
                _machine = value;

            }
        }
    }
}
