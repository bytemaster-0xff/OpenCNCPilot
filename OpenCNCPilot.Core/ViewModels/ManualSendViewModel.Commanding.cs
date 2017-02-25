using LagoVista.Core.Commanding;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class ManualSendViewModel
    {
        private void InitCommands()
        {
            ManualSendCommand = new RelayCommand(ManualSend, CanManualSend);
            Machine.PropertyChanged += Machine_PropertyChanged;
        }

        private void Machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ManualSendCommand.RaiseCanExecuteChanged();
        }

        public bool CanManualSend()
        {
            return Machine.Connected && Machine.Mode == OperatingMode.Manual;
        }

        public bool CanShowPrevious()
        {
            return _commandBufferLocation > 0;
        }

        public bool CanShowNext()
        {
            return _commandBufferLocation < _commandBuffer.Count - 1;
        }

        public RelayCommand ManualSendCommand { get; private set; }

        public RelayCommand ShowPreviousCommand { get; private set; }

        public RelayCommand ShowNextCommand { get; private set; }
    }
}
