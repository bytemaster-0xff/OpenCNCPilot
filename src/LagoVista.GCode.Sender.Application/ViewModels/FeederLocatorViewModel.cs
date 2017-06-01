using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.PickAndPlace.Models;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public class FeederLocatorViewModel : MachineVisionViewModelBase
    {
        PnPJob _job;

        /* Goal of this is to find the bottom XY of the tray and then find the initial position of the part of each populated row. */

        public FeederLocatorViewModel(IMachine machine, PnPJob job) : base(machine)
        {
            _job = job;
        }


        public override async Task InitAsync()
        {
            await base.InitAsync();
        }

        public override void CircleLocated(Point2D<double> point, double diameter, Point2D<double> stdDev)
        {
            Machine.BoardAlignmentManager.CircleLocated(point);
        }

        public override void CornerLocated(Point2D<double> point, Point2D<double> stdDev)
        {
            Machine.BoardAlignmentManager.CornerLocated(point);
        }


        public PnPJob Job { get { return _job; } }
    }
}
