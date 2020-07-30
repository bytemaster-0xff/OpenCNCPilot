using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LagoVista.Core;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LagoVista.GCode.Sender.Application.ViewModels
{
    public class MVPNPViewModel : MachineVisionViewModelBase
    {

        public MVPNPViewModel(IMachine machine) : base(machine)
        {
            PickPartFromBoardCommand = new RelayCommand(PickPartFromBoard);
            PlacePartOnBoardCommand = new RelayCommand(PlacePartOnBoard);

            PickPartFromTapeCommand = new RelayCommand(PickPartFromTape);
            PlacePartOnTapeCommand = new RelayCommand(PlacePartOnTape);

            FullCycleCommand = new RelayCommand(FullCycle);
        }

        public async override Task InitAsync()
        {
            _trayHeight = Convert.ToDouble(await Storage.GetKVPAsync<string>("TRAY_HEIGHT", "63.5"));
            _boardHeight = Convert.ToDouble(await Storage.GetKVPAsync<string>("BOARD_HEIGHT", "71.4"));

            _destinationX = Convert.ToDouble(await Storage.GetKVPAsync<string>("DESTX", "0"));
            _destinationY = Convert.ToDouble(await Storage.GetKVPAsync<string>("DESTY", "0"));

            _jogSpeed = Convert.ToDouble(await Storage.GetKVPAsync<string>("JOG", "2000"));
            _plunge = Convert.ToDouble(await Storage.GetKVPAsync<string>("PLUNGE", "1000"));

            RaisePropertyChanged(nameof(TrayHeight));
            RaisePropertyChanged(nameof(BoardHeight));

            RaisePropertyChanged(nameof(DestinationY));
            RaisePropertyChanged(nameof(DestinationX));

            RaisePropertyChanged(nameof(Plunge));
            RaisePropertyChanged(nameof(JogSpeed));

            await base.InitAsync();

            StartCapture();
        }

        public void PickPartFromBoard()
        {
            Machine.SendCommand("M63 P0");
            Machine.SendCommand("G04 P1000");
            Machine.SendCommand($"G0 Z{BoardHeight}");
            Machine.SendCommand("G04 P1000");
            Machine.SendCommand("M63 P1");
            Machine.SendCommand("G04 P1000");
            Machine.SendCommand("G0 Z50");
        }

        public void PickPartFromTape()
        {
            _originalPickLocation = new Vector2(Machine.NormalizedPosition.X, Machine.NormalizedPosition.Y);

            Machine.SendCommand("M63 P0");
            Machine.SendCommand("G04 P1000");
            Machine.SendCommand($"G0 Z{TrayHeight}");
            Machine.SendCommand("G04 P1000");
            Machine.SendCommand("M63 P1");
            Machine.SendCommand("G04 P1000");
            Machine.SendCommand("G0 Z50");
        }

        public void PlacePartOnTape()
        {
            Machine.SendCommand($"G0 Z{TrayHeight}");
            Machine.SendCommand("G04 P1000");
            Machine.SendCommand("M63 P0");
            Machine.SendCommand("M64 P1");
            Machine.SendCommand("G04 P1000");
            Machine.SendCommand("G0 Z50");
        }

        public void PlacePartOnBoard()
        {
            /*    Machine.SendCommand($"G0 Z{BoardHeight}");
                Machine.SendCommand("G04 P1000");
                Machine.SendCommand("M63 P0");
                Machine.SendCommand("M64 P1");
                Machine.SendCommand("G04 P1000");
                Machine.SendCommand("G0 Z50");*/

            bool on = true;
            var bldr = new StringBuilder();
            for (var idx = 0; idx < 30; ++idx)
            {
                bldr.AppendLine("G0 X50 Y50 Z10 F2500");
                bldr.AppendLine("G0 X150.123 Y50.546");
                bldr.AppendLine("G0 X150.27 Y150.893");
                bldr.AppendLine("G0 X50 Y150");
                /*                bldr.AppendLine("G4 P500");
                                if (on)
                                    bldr.AppendLine("M62 P0");
                                else
                                    bldr.AppendLine("M62 P1");*/
                bldr.AppendLine("G0 X50.852 Y50.534");

                on = !on;
            }

            Machine.GCodeFileManager.SetGCode(bldr.ToString());
        }

        public void FullCycle()
        {
            _originalPickLocation = new Vector2(Machine.NormalizedPosition.X, Machine.NormalizedPosition.Y);

            var bldr = new StringBuilder();
            bldr.AppendLine("T0");
            bldr.AppendLine("G28 Z");
            bldr.AppendLine("M62 P1");
            bldr.AppendLine("M64 P0"); /* Close Exahust */
            bldr.AppendLine("M63 P0");
            bldr.AppendLine("M60 P1");
            bldr.AppendLine("G04 P500");

            for (var idx = 0; idx < 5; ++idx)
            {
                bldr.AppendLine($"G0 X{_originalPickLocation.X.ToDim()} Y{_originalPickLocation.Y.ToDim()} Z50 F{JogSpeed}");
                bldr.AppendLine($"G0 Z{TrayHeight.ToDim()} F{Plunge}");
                bldr.AppendLine("G04 P100");
                bldr.AppendLine("M63 P1"); /* Turn on vacuum */
                bldr.AppendLine("G04 P100");
                bldr.AppendLine("G0 Z50");
                bldr.AppendLine($"G0 X{DestinationX.ToDim()} Y{DestinationY.ToDim()} F{JogSpeed}");
                bldr.AppendLine($"G0 Z{BoardHeight} F{Plunge}");
                bldr.AppendLine("M63 P0"); /* Close off vacuum */
                bldr.AppendLine("M64 P1"); /* Open exahaust */
                bldr.AppendLine("G04 P250"); /* Wait for part drop */
                bldr.AppendLine("M64 P0"); /* Close exahaust */
                bldr.AppendLine($"G0 Z50 F{Plunge}"); /* Move Head Up */
                bldr.AppendLine($"G0 Z{BoardHeight} F{Plunge}"); /* Move to board */
                bldr.AppendLine("M63 P1"); /* turn on vacuum */
                bldr.AppendLine("G04 P150");
                bldr.AppendLine("G0 Z50 F{Plunge}"); ; /* Pick up part */
                bldr.AppendLine($"G0 X{_originalPickLocation.X.ToDim()} Y{_originalPickLocation.Y.ToDim()} F{JogSpeed}");
                bldr.AppendLine($"G0 Z{TrayHeight} F{Plunge}"); /* Move down to tray */
                bldr.AppendLine("G04 P100");
                bldr.AppendLine("M63 P0"); /* Turn off vacuum */
                bldr.AppendLine("M64 P1"); /* Open exaust */
                bldr.AppendLine("G04 P100"); 
                bldr.AppendLine("G0 Z50 F{Plunge}"); /* Return to origin Z */
            }

            bldr.AppendLine("M62 P0"); /* Turn off pump */
            bldr.AppendLine("M60 P0"); /* Turn off light */

            Machine.GCodeFileManager.SetGCode(bldr.ToString());
        }

        Vector2 _originalPickLocation;

        public ICommand PickPartFromBoardCommand { get; private set; }

        public ICommand PickPartFromTapeCommand { get; private set; }

        public ICommand PlacePartOnBoardCommand { get; private set; }

        public ICommand PlacePartOnTapeCommand { get; private set; }
        public ICommand FullCycleCommand { get; private set; }

        private double _trayHeight = 63.5;
        public double TrayHeight
        {
            get { return _trayHeight; }
            set
            {
                Set(ref _trayHeight, value);
                Storage.StoreKVP("TRAY_HEIGHT", value.ToString());
            }
        }

        private double _boardHeight = 63.5;
        public double BoardHeight
        {
            get { return _boardHeight; }
            set
            {
                Set(ref _boardHeight, value);
                Storage.StoreKVP("BOARD_HEIGHT", value.ToString());
            }
        }

        private double _destinationX;
        public double DestinationX
        {
            get { return _destinationX; }
            set
            {
                Set(ref _destinationX, value);
                Storage.StoreKVP("DESTX", value.ToString());
            }
        }


        private double _destinationY;
        public double DestinationY
        {
            get { return _destinationY; }
            set
            {
                Set(ref _destinationY, value);
                Storage.StoreKVP("DESTY", value.ToString());
            }
        }

        private double _jogSpeed;
        public double JogSpeed
        {
            get { return _jogSpeed; }
            set
            {
                Set(ref _jogSpeed, value);
                Storage.StoreKVP("JOG", value.ToString());
            }
        }

        private double _plunge;
        public double Plunge
        {
            get { return _plunge; }
            set
            {
                Set(ref _plunge, value);
                Storage.StoreKVP("PLUNGE", value.ToString());
            }
        }


    }
}
