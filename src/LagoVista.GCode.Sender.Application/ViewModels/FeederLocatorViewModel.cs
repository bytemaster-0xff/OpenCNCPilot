using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public class FeederLocatorViewModel : MachineVisionViewModelBase
    {
        public FeederLocatorViewModel(IMachine machine) : base(machine)
        {

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
    }
}
