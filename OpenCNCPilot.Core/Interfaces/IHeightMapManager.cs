using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Models;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Interfaces
{
    public interface IHeightMapManager : INotifyPropertyChanged
    {
        HeightMap HeightMap { get; }

        bool HasHeightMap { get; }
        bool HeightMapDirty { get; }

        void ProbeCompleted(Vector3 probeResult);
        void NewHeightMap(HeightMap heightMap);
        Task OpenHeightMapAsync(string path);
        Task SaveHeightMapAsync(string path);
        Task SaveHeightMapAsync();
        void CloseHeightMap();

        void CreateTestPattern();

        void StartProbing();
        void PauseProbing();
        void CancelProbing();
    }
}
