using LagoVista.GCode.Sender.Models;

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
    }
}
