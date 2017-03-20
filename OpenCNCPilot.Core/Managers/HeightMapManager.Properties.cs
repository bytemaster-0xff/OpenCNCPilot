using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Models;
using System.Collections.ObjectModel;

namespace LagoVista.GCode.Sender.Managers
{
    public partial class HeightMapManager
    {
        private HeightMap _heightMap;
        public HeightMap HeightMap
        {
            get { return _heightMap; }
            set
            {
                _heightMap = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasHeightMap));
                RaisePropertyChanged(nameof(Status));
            }
        }

        public HeightMapStatus Status
        {
            get
            {
                return (HasHeightMap) ? HeightMap.Status : HeightMapStatus.NotAvailable;
            }
            set
            {
                if(HasHeightMap)
                {
                    HeightMap.Status = value;
                }

                RaisePropertyChanged();
            }
        }

        public bool HasHeightMap
        {
            get { return _heightMap != null; }
        }

        private bool _heightMapDirty = false;
        public bool HeightMapDirty
        {
            get { return _heightMapDirty; }
            set
            {
                _heightMapDirty = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<LagoVista.Core.Models.Drawing.Line3D> RawBoardOutline { get; private set; }

        /// <summary>
        /// The XY Coordinates of the points that will be probed.
        /// </summary>
        public ObservableCollection<Vector3> ProbePoints { get; private set; }
    }
}
