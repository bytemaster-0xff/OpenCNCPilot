using LagoVista.GCode.Sender.ViewModels;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LagoVista.GCode.Sender.Application.Controls
{
    /// <summary>
    /// Interaction logic for JogControls.xaml
    /// </summary>
    public partial class JogControls : UserControl
    {

        Controller _controller;
        State? _lastState;

        Timer _timer;

        MachineControlViewModel _viewModel;

        public JogControls()
        {
            InitializeComponent();

            this.Loaded += JogButtons_Loaded;

            _timer = new Timer(ReadController, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void JogButtons_Loaded(object sender, RoutedEventArgs e)
        {
            _controller = new Controller(UserIndex.One);
            _viewModel = this.DataContext as MachineControlViewModel;
            _timer.Change(0, 50);
        }

        bool IsPressed(State state, GamepadButtonFlags btn)
        {
            return ((state.Gamepad.Buttons & btn) == btn);
        }

        bool WasPressed(State lastState, State thisState, GamepadButtonFlags btn)
        {
            return (!IsPressed(lastState, btn) && IsPressed(thisState, btn));
        }

        void ReadController(object state)
        {
            if (_controller.IsConnected &&
                _viewModel.Machine.Connected &&
                _viewModel.Machine.Mode == OperatingMode.Manual)
            {
                var controllerState = _controller.GetState();
                if (_lastState.HasValue)
                {
                    var btn = controllerState.Gamepad.Buttons;
                    if (WasPressed(_lastState.Value, controllerState, GamepadButtonFlags.A))
                    {
                        _viewModel.XYStepMode = StepModes.Small;
                        _viewModel.ZStepMode = StepModes.Micro;
                    }

                    if (WasPressed(_lastState.Value, controllerState, GamepadButtonFlags.X))
                    {
                        _viewModel.XYStepMode = StepModes.Medium;
                        _viewModel.ZStepMode = StepModes.Small;
                    }

                    if (WasPressed(_lastState.Value, controllerState, GamepadButtonFlags.B))
                    {
                        _viewModel.XYStepMode = StepModes.Large;
                        _viewModel.ZStepMode = StepModes.Medium;
                    }

                    if (WasPressed(_lastState.Value, controllerState, GamepadButtonFlags.Y))
                    {
                        _viewModel.XYStepMode = StepModes.XLarge;
                        _viewModel.ZStepMode = StepModes.Large;
                    }

                    if (WasPressed(_lastState.Value, controllerState, GamepadButtonFlags.DPadDown))
                    {
                        _viewModel.Jog(JogDirections.YMinus);
                    }

                    if (WasPressed(_lastState.Value, controllerState, GamepadButtonFlags.DPadUp))
                    {
                        _viewModel.Jog(JogDirections.YPlus);
                    }

                    if (WasPressed(_lastState.Value, controllerState, GamepadButtonFlags.DPadLeft))
                    {
                        _viewModel.Jog(JogDirections.XMinus);
                    }

                    if (WasPressed(_lastState.Value, controllerState, GamepadButtonFlags.DPadRight))
                    {
                        _viewModel.Jog(JogDirections.XPlus);
                    }

                    if (WasPressed(_lastState.Value, controllerState, GamepadButtonFlags.LeftShoulder))
                    {
                        _viewModel.Jog(JogDirections.ZMinus);
                    }

                    if (WasPressed(_lastState.Value, controllerState, GamepadButtonFlags.RightShoulder))
                    {
                        _viewModel.Jog(JogDirections.ZPlus);
                    }
                }

                _lastState = controllerState;
            }
        }
    }
}
