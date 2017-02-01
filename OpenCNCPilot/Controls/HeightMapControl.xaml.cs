using LagoVista.Core.GCode.Commands;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;


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
          //  ModelTool.Point1 = (App.Current.Machine.WorkPosition + new Vector3(0, 0, 10)).ToPoint3D().ToMedia3D();
          //  ModelTool.Point2 = App.Current.Machine.WorkPosition.ToPoint3D().ToMedia3D();
        }

        public bool ModelToolVisible
        {
            get { return ModelTool.Visible; }
            set { ModelTool.Visible = value; }
        }

        public static readonly DependencyProperty CurrentJobProperty
                = DependencyProperty.Register("CurrentJob", typeof(IJobProcessor), typeof(HeightMapControl), new PropertyMetadata(null));

        public IJobProcessor CurrentJob
        {
            get { return GetValue(CurrentJobProperty) as IJobProcessor; }
            set
            {
                SetValue(CurrentJobProperty, value);
              /*  if (value != null)
                    Presentation.HeightMapServices.GetModel(value.Commands, App.Current.Settings, ModelLine, ModelRapid, ModelArc);
                else
                    Clear();*/
            }
        }

        public static readonly DependencyProperty HeightMapProperty
            = DependencyProperty.Register("HeightMap", typeof(HeightMap), typeof(HeightMapControl), new PropertyMetadata(null));

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
