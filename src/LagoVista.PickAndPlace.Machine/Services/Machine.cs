using LagoViata.PNP.Channel;
using LagoViata.PNP.Drivers;
using LagoVista.Core.GCode.Commands;
using LagoVista.Core.GCode.Parser;
using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Devices.Gpio;

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

        public Machine(IGCodeParser parser, IConnection connection)
        {
            _connection = connection;
            _connection.ImmediateCommandReceivedEvent += _connection_ImmediateCommandReceivedEvent;
            _connection.LineReceivedEvent += _connection_LineReceivedEvent;
            _parser = parser;
            _axes = new List<Axis>();
            _commandBuffer = new ConcurrentQueue<GCodeCommand>();

            _yAxis = new Axis(21, 20);
            _xAxis = new Axis(19, 13);

            _cAxis = new Axis(6, 5);
            _placeAxis = new Axis(22, 27);
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

        public void HandleGCodeLine(String gcode)
        {
            var cmd = _parser.ParseLine(gcode, 0);
            if(cmd != null)
            {
                _commandBuffer.Enqueue(cmd);
            }
        }

        public async Task InitAsync()
        {
            if (LightningProvider.IsLightningEnabled)
            {
                var gpioController = (await GpioController.GetControllersAsync(LightningGpioProvider.GetGpioProvider()))[0];
                foreach(var axis in _axes)
                {
                    axis.Init(gpioController);
                }

                _motorPower.Init(gpioController);
            }

            _motorPower.Enable();
        }

        GCodeCommand _curent;

        public void HandleCommand(GCodeCommand cmd)
        {
            _curent = cmd;
        }

        private void UpdateLoop()
        {
            
            long totalMicroSeconds = 0;
            long lastMilliseconds = 0;
            var sw = new System.Diagnostics.Stopwatch();
            var pauseSW = new System.Diagnostics.Stopwatch();
            sw.Start();
        }

        public void StartWorkLoop()
        {
            UpdateLoop();

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