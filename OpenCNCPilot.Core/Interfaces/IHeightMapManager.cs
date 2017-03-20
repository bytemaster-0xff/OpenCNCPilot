using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Interfaces
{
    public interface IHeightMapManager : INotifyPropertyChanged
    {
        /// <summary>
        /// The current Height Map
        /// </summary>
        HeightMap HeightMap { get; }

        /// <summary>
        /// Returns true if we have a height map associated with the board.
        /// </summary>
        bool HasHeightMap { get; }
        bool HeightMapDirty { get; }

        HeightMapStatus Status { get; }


        void NewHeightMap(HeightMap heightMap);


        void ProbeCompleted(Vector3 probeResult);
        Task OpenHeightMapAsync(string path);
        Task SaveHeightMapAsync(string path);
        Task SaveHeightMapAsync();
        void CloseHeightMap();

        void CreateTestPattern();

        /// <summary>
        /// Start a Height Map Probing Job
        /// </summary>
        void StartProbing();

        /// <summary>
        /// Pause the current Height Map Probing Job
        /// </summary>
        void PauseProbing();

        /// <summary>
        /// Cancel the current Height Map Probing Job
        /// </summary>
        void CancelProbing();

        /// <summary>
        /// Outline of the PCB Blank
        /// </summary>
        ObservableCollection<LagoVista.Core.Models.Drawing.Line3D> RawBoardOutline { get; }

        /// <summary>
        /// The XY Coordinates of the points that will be probed.
        /// </summary>
        ObservableCollection<Vector3> ProbePoints { get; }

    }
}
