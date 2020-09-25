using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.GCode.Sender.Interfaces;
using System;
using System.Collections.Generic;

namespace LagoVista.GCode.Sender.ViewModels
{
    public class GCodeAppViewModelBase : ViewModelBase
    {
        private IMachine _machine;

        public GCodeAppViewModelBase(IMachine machine)
        {
            _machine = machine;

            InitCommands();
        }

        public GCodeAppViewModelBase()
        {

        }

        public IMachine Machine { get { return _machine; } set { _machine = value; } }

        public void AssertInManualMode(Action action)
        {
            if (Machine.Mode != OperatingMode.Manual)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Machine Busy");
            }
            else
            {
                action();
            }            
        }

        private int _commandBufferLocation = 0;
        private List<String> _commandBuffer = new List<string>();

        public void ManualSend()
        {
            _commandBuffer.Add(ManualCommandText);
            _commandBufferLocation = _commandBuffer.Count;
            if (!String.IsNullOrEmpty(ManualCommandText))
            {
                Machine.SendCommand(ManualCommandText);
                ManualCommandText = String.Empty;
            }
        }

        public void ShowPrevious()
        {
            if (CanShowPrevious())
            {
                --_commandBufferLocation;
                ManualCommandText = _commandBuffer[_commandBufferLocation];
            }
        }

        public void ShowNext()
        {
            if (CanShowNext())
            {
                ++_commandBufferLocation;
                ManualCommandText = _commandBuffer[_commandBufferLocation];
            }
            else
            {
                ++_commandBufferLocation;
                _commandBufferLocation = Math.Min(_commandBufferLocation, _commandBuffer.Count);
                ManualCommandText = string.Empty;
            }
        }

        private String _manualCommandText;
        public String ManualCommandText
        {
            get { return _manualCommandText; }
            set
            {
                Set(ref _manualCommandText, value);
            }
        }

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
            return _commandBuffer.Count > 0 && _commandBufferLocation > 0;
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
