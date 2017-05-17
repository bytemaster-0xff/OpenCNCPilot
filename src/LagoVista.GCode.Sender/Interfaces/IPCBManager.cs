using LagoVista.Core.Models.Drawing;
using LagoVista.EaglePCB.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Interfaces
{
    public interface IPCBManager : INotifyPropertyChanged
    {
        bool HasBoard { get; }

        bool HasTopEtching { get; }

        bool HasBottomEtching { get; }

        PCB Board { get; }

        Task<bool> OpenFileAsync(string boardFile);
        Point2D<double> FirstFiducial { get; set; }

        Point2D<double> SecondFiducial { get; set; }

        Task<bool> OpenProjectAsync(string projectFile);


        bool HasProject { get; }

        PCBProject Project { get; set; }

        String ProjectFilePath { get; set; }

        void SetMeasuredOffset(Point2D<double> offset, double angleDegrees);

        void ClearMeasuredOffset();

        double MeasuredOffsetAngle { get; }

        Point2D<double> MeasuredOffset { get; }

        /// <summary>
        /// If there is an offset and angle stored, apply that to the point, otherwise return original point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        Point2D<double> GetAdjustedPoint(Point2D<double> point);

        bool CameraNavigation { get; set; }

        bool Tool1Navigation { get; set; }

        bool Tool2Navigation { get; set; }

        bool IsSetFiducialMode { get; set; }

        bool IsNavigationMode { get; set; }
    }
}
