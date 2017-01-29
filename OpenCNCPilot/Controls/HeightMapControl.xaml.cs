using LagoVista.Core.GCode.Commands;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Application.Presentation;
using System.Collections.Generic;
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
        }

        public void SetNewModel(Vector2 min, Vector2 max, double gridSize)
        {
            HeightMap.GetPreviewModel(min, max, gridSize, ModelHeightMapBoundary, ModelHeightMapPoints);
        }

        public void SetPreviewModel(HeightMap map)
        {
            map.GetPreviewModel(ModelHeightMapBoundary, ModelHeightMapPoints);
        }

    
        public void GetModel(HeightMap map)
        {
            map.GetModel(ModelHeightMap);
        }

        public void Clear()
        {
            ModelHeightMap.MeshGeometry = new System.Windows.Media.Media3D.MeshGeometry3D();
            ModelHeightMapBoundary.Points.Clear();
            ModelHeightMapPoints.Points.Clear();
        }

        public void SetPreviewModel(IReadOnlyList<Command> commands)
        {
            HeightMap.GetModel(commands, App.Current.Settings, ModelLine, ModelRapid, ModelArc);
        }


        public void SetPreviewModel(List<Command> commands)
        {
            HeightMap.GetModel(commands, App.Current.Settings, ModelLine, ModelRapid, ModelArc);
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
    }
}
