using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.PickAndPlace.Models;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public class FeederLocatorViewModel : MachineVisionViewModelBase
    {
        PnPJob _job;
        string _pnpJobFileName;

        /* Goal of this is to find the bottom XY of the tray and then find the initial position of the part of each populated row. */

        public FeederLocatorViewModel(IMachine machine, PnPJob job, string pnpJobFileName) : base(machine)
        {
            _job = job;
            _pnpJobFileName = pnpJobFileName;
            SaveCommand = new RelayCommand(Save);
        }

        public async void Save()
        {
            await Storage.StoreAsync(Job, _pnpJobFileName);
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

        private FeederInstance _feederInstance;
        public FeederInstance SelectedFeeder
        {
            get { return _feederInstance; }
            set { Set(ref _feederInstance, value); }
        }


        private Row _selectedRow;
        public Row SelectedRow
        {
            get { return _selectedRow; }
            set { Set(ref _selectedRow, value); }
        }

        public RelayCommand SaveCommand { get; private set; }
    }
}
