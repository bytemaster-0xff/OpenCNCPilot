using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public class CalibrationViewModel : MachineVisionViewModelBase
    {
        public CalibrationViewModel(IMachine machine) : base(machine)
        {

        }

        public override async Task InitAsync()
        {
            await base.InitAsync();

        }

        public override void CircleLocated(Point2D<double> point, double diameter)
        {
            Machine.BoardAlignmentManager.CircleLocated(point);
        }

        public override void CornerLocated(Point2D<double> point)
        {
            Machine.BoardAlignmentManager.CornerLocated(point);
        }
    }
}