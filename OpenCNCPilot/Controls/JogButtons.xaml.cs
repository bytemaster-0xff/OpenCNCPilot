using System.Windows;
using System.Windows.Controls;
using SharpDX.XInput;
using System.Threading;
using System.Diagnostics;

namespace LagoVista.GCode.Sender.Application.Controls
{
    /// <summary>
    /// Interaction logic for JogButtons.xaml
    /// </summary>
    public partial class JogButtons : UserControl
    {
        Controller _controller;
        State? _lastState;

        Timer _timer;

        public JogButtons()
        {
            InitializeComponent();

            this.Loaded += JogButtons_Loaded;

            _timer = new Timer(ReadController, null, Timeout.Infinite, Timeout.Infinite);

        }

        private void JogButtons_Loaded(object sender, RoutedEventArgs e)
        {
            _controller = new Controller(UserIndex.One);

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
            if (_controller.IsConnected)
            {
                var controllerState = _controller.GetState();
                if (_lastState.HasValue)
                {
                    var btn = controllerState.Gamepad.Buttons;
                    if (WasPressed(_lastState.Value, controllerState, GamepadButtonFlags.A))
                    {
                        Debug.WriteLine("A Pressed");
                    }
                }

                _lastState = controllerState;
            }
        }
    }
}
