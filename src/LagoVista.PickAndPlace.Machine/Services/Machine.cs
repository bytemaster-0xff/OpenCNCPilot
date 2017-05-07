using LagoViata.PNP.Channel;
using LagoViata.PNP.Drivers;
using LagoVista.Core.GCode.Commands;
using LagoVista.Core.GCode.Parser;
using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Devices.Gpio;
using Windows.Foundation.Collections;

// May want to consider rebuilding and repackaging 
// https://github.com/ms-iot/lightning

namespace LagoViata.PNP.Services
{
    public class Machine
    {
        readonly Axis _xAxis;
        readonly Axis _yAxis;
        readonly Axis _placeAxis;
        readonly Axis _solderAxis;
        readonly Axis _cAxis;

        readonly MotorPower _motorPower;

        readonly IGCodeParser _parser;

        IConnection _connection;

        //Plural for axis is axes...yup...
        readonly List<Axis> _axes;
        readonly ConcurrentQueue<GCodeCommand> _commandBuffer;

        AppServiceConnection _appServiceConnection;
        AppService _appService;

        public Machine(IGCodeParser parser, IConnection connection)
        {
            _connection = connection;
            _connection.ImmediateCommandReceivedEvent += _connection_ImmediateCommandReceivedEvent;
            _connection.LineReceivedEvent += _connection_LineReceivedEvent;
            _parser = parser;
            _axes = new List<Axis>();
            _commandBuffer = new ConcurrentQueue<GCodeCommand>();

            _xAxis = new Axis(19, 0);
            _yAxis = new Axis(21, 1);
            _cAxis = new Axis(6, 2);
            _placeAxis = new Axis(22, 3);
            _solderAxis = new Axis(17, 4);

            _axes.Add(_xAxis);
            _axes.Add(_yAxis);
            _axes.Add(_cAxis);
            _axes.Add(_placeAxis);
            _axes.Add(_solderAxis);

            _motorPower = new MotorPower(26);
        }

        private void _connection_LineReceivedEvent(object sender, string cmd)
        {
            HandleGCodeLine(cmd);
        }

        public void Kill()
        {

        }

        public void SendStatus()
        {

        }

        public void SoftReset()
        {

        }

        public void ClearAlarm()
        {

        }

        private void _connection_ImmediateCommandReceivedEvent(object sender, char e)
        {
            switch(e)
            {
                case '!': Kill(); break;
                case '?': SendStatus(); break;
                case (char)0x06: ClearAlarm(); break;
                case (char)0x18: SoftReset(); break;
            }
        }

        public async void HandleGCodeLine(String gcode)
        {

            var cmd = _parser.ParseLine(gcode, 0);
            if(cmd != null)
            {
                _commandBuffer.Enqueue(cmd);

            }
        }

        public async Task InitAsync()
        {
            var listing = await AppServiceCatalog.FindAppServiceProvidersAsync("com.softwarelogistics.gcodemachine");
            if (listing != null && listing.Any())
            {
                var packageName = listing[0].PackageFamilyName;
                Debug.WriteLine("Found Package - Opening: " + packageName);

                _appServiceConnection = new AppServiceConnection();
                _appServiceConnection.PackageFamilyName = packageName;
                _appServiceConnection.AppServiceName = "com.softwarelogistics.gcodemachine";

                var status = await _appServiceConnection.OpenAsync();
                if (status == AppServiceConnectionStatus.Success)
                {
                    Debug.WriteLine("Success Opening");
                    _appServiceConnection.RequestReceived += _appServiceConnection_RequestReceived;
                }
                else
                {
                    Debug.WriteLine("Failed Opening " + status.ToString());
                }

                _appService = new AppService(_appServiceConnection);
            }
            else
            {
                Debug.Write("Could not connect to background service.");
            }

            if (LightningProvider.IsLightningEnabled)
            {
                var gpioController = (await GpioController.GetControllersAsync(LightningGpioProvider.GetGpioProvider()))[0];
                foreach (var axis in _axes)
                {
                    axis.Init(gpioController, _appService);
                }

                _motorPower.Init(gpioController);
            }

            _motorPower.Enable();
        }


        private void _appServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            if (args.Request.Message.ContainsKey("STATUS"))
            {
                var msg = args.Request.Message["STATUS"];
                Debug.WriteLine("STATUS: " + msg);
            }

            if (args.Request.Message.ContainsKey("DONE"))
            {
                var axis = Convert.ToInt32(args.Request.Message["DONE"]);
                switch(axis)
                {
                    case 0: XAxis.Completed(); break;
                    case 1: YAxis.Completed(); break;
                    case 2: CAxis.Completed(); break;
                    case 3: PlaceAxis.Completed(); break;
                    case 4: SolderAxis.Completed(); break;
                }
            }
        }

        GCodeCommand _curent;

        public void HandleCommand(GCodeCommand cmd)
        {
            _curent = cmd;
            var movement = cmd as GCodeLine;
            if(movement != null)
            {
                CAxis.Move(movement.End.Y, Convert.ToDouble(movement.Feed));
            }
        }
        

        public void StartWorkLoop()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    bool isBusy = _axes.Where(axis => axis.IsBusy == true).Any();

                    if (isBusy || _curent != null)
                    {
                        _curent = null;
                    }

                    if (!isBusy)
                    {
                        GCodeCommand cmd;
                        if (_commandBuffer.TryDequeue(out cmd))
                        {
                            HandleCommand(cmd);
                        }
                    }
                }
            });
        }

        public Axis XAxis { get { return _xAxis; } }
        public Axis YAxis { get { return _yAxis; } }
        public Axis CAxis { get { return _cAxis; } }
        public Axis PlaceAxis { get { return _placeAxis; } }
        public Axis SolderAxis { get { return _solderAxis; } }
    }
}