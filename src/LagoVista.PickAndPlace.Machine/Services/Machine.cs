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

        Accessory _topLight;
        Accessory _bottomLight;
        Accessory _vacuum;

        enum Tools
        {
            PlaceHead = 0,
            PasteHead = 1,
            CAxis = 2
        }

        String _state = "Idle";

        Tools _currentTool;

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

            _xAxis = new Axis(0, 20, 7, 8);
            _yAxis = new Axis(1, 13, 16, 12);
            _cAxis = new Axis(2, 5);
            _placeAxis = new Axis(3, 27, 25);
            _solderAxis = new Axis(4, 4, 24);

            _vacuum = new Accessory(10);
            _bottomLight = new Accessory(11);
            _topLight = new Accessory(9);

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

        public async void SendStatus()
        {
            var bldr = new StringBuilder();
            bldr.Append($"<{_state},");
            bldr.Append($"MPos:{XAxis.CurrentLocation:0.0000},{YAxis.CurrentLocation:0.0000},{_placeAxis.CurrentLocation:0.0000},{_solderAxis.CurrentLocation:0.0000},{_cAxis.CurrentLocation:0.0000},");
            bldr.Append($"WPos:{XAxis.WorkOffset:0.0000},{YAxis.WorkOffset:0.0000},{_placeAxis.WorkOffset:0.0000},{_solderAxis.WorkOffset:0.0000},{_cAxis.WorkOffset:0.0000},");
            bldr.Append($"T:{(int)_currentTool},");
            bldr.Append($"P:{(int)0}>\n");
            await _connection.WriteAsync(bldr.ToString());
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

        public void HandleGCodeLine(String gcode)
        {
            if (gcode == "T0")
            {
                _currentTool = Tools.PlaceHead;
            }
            else if (gcode == "T1")
            {
                _currentTool = Tools.PasteHead;
            }
            else if (gcode == "T2")
            {
                _currentTool = Tools.CAxis;
            }
            else
            {
                var cmd = _parser.ParseLine(gcode, 0);
                if (cmd != null)
                {
                    _commandBuffer.Enqueue(cmd);
                }
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

                _topLight.Init(gpioController);
                _bottomLight.Init(gpioController);
                _vacuum.Init(gpioController);

                _motorPower.Init(gpioController);
                _motorPower.Enable();
            }
            else
            {
                Debug.WriteLine("Ligtning IS NOT Enabled, please enable through Device Portal");
            }

            StartWorkLoop();
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
                Debug.WriteLine($"FINISHED {axis}");
                switch (axis)
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
                XAxis.Move(movement.End.X, Convert.ToDouble(movement.Feed));
                YAxis.Move(movement.End.Y, Convert.ToDouble(movement.Feed));
                switch(_currentTool)
                {
                    case Tools.CAxis: CAxis.Move(movement.End.Z, Convert.ToDouble(movement.Feed)); break;
                    case Tools.PasteHead: SolderAxis.Move(movement.End.Z, Convert.ToDouble(movement.Feed)); break;
                    case Tools.PlaceHead: PlaceAxis.Move(movement.End.Z, Convert.ToDouble(movement.Feed)); break;
                }              
            }

            var machine = cmd as MCode;
            if (machine != null)
            {
                switch (machine.Code)
                {
                    case 60: if (machine.Power == 0) _topLight.Off(); else _topLight.On(); break;
                    case 61: if (machine.Power == 0) _bottomLight.Off(); else _bottomLight.On(); break;
                    case 62: if (machine.Power == 0) _vacuum.Off(); else _vacuum.On(); break;
                }
            }
        }
        

        public void StartWorkLoop()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    bool isBusy = _axes.Where(axis => axis.IsBusy == true).Any();
                    foreach(var axis in _axes)
                    {
                        axis.Update(DateTime.Now.Ticks);
                    }

                    if (!isBusy && _curent != null)
                    {
                        _connection.WriteAsync("ok");
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