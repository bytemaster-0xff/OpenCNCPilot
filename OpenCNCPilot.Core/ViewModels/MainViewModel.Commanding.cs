﻿using LagoVista.Core.Commanding;
using LagoVista.Core.GCode;
using System;

namespace LagoVista.GCode.Sender.ViewModels
{
    public partial class MainViewModel
    {
        private void InitCommands()
        {
            OpenHeightMapCommand = new RelayCommand(OpenHeightMapFile, CanPerformFileOperation);
            OpenGCodeFileCommand = new RelayCommand(OpenGCodeFile, CanPerformFileOperation);
            CloseFileCommand = new RelayCommand(CloseFile, CanPerformFileOperation);
            ClearHeightMapCommand = new RelayCommand(ClearHeightMap, CanClearHeightMap);
            ArcToLineCommand = new RelayCommand(ArcToLine, CurrentlyJob);
            ApplyHeightMapCommand = new RelayCommand(ApplyHeightMap, CanApplyHeightMap);

            SetMetricUnitsCommand = new RelayCommand(SetMetricUnits, CanChangeUnits);
            SetImperialUnitsCommand = new RelayCommand(SetImperialUnits, CanChangeUnits);

            SetAbsolutePositionModeCommand = new RelayCommand(SetAbsolutePositionMode, CanSetPositionMode);
            SetIncrementalPositionModeCommand = new RelayCommand(SetIncrementalPositionMode, CanSetPositionMode);

            Machine.PropertyChanged += _machine_PropertyChanged;
        }

        private void _machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.Mode))
            {
                DispatcherServices.Invoke(() =>
                {
                    OpenHeightMapCommand.RaiseCanExecuteChanged();
                    OpenGCodeFileCommand.RaiseCanExecuteChanged();
                    CloseFileCommand.RaiseCanExecuteChanged();
                    SetMetricUnitsCommand.RaiseCanExecuteChanged();
                    SetImperialUnitsCommand.RaiseCanExecuteChanged();
                });
            }

            if (e.PropertyName == nameof(Machine.HasJob))
            {
                ArcToLineCommand.RaiseCanExecuteChanged();
                ApplyHeightMapCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CurrentlyJob()
        {
            return Machine.HasJob;
        }

        public bool CanSetPositionMode()
        {
            return Machine.Mode == OperatingMode.Manual && Machine.Connected;
        }

        public bool CanChangeUnits()
        {
            return Machine.Mode == OperatingMode.Manual && Machine.Connected;
        }
       
        public bool CanApplyHeightMap()
        {
            return Machine.HasJob && HeightMapVM.CurrentHeightMap != null;
        }

        private bool CanPerformFileOperation(Object instance)
        {
            return (Machine.Mode != OperatingMode.SendingJob);
        }

        public bool CanClearHeightMap()
        {
            return HeightMapVM.CurrentHeightMap != null;
        }

        public RelayCommand SetImperialUnitsCommand { get; private set; }
        public RelayCommand SetMetricUnitsCommand { get; private set; }

        public RelayCommand SetAbsolutePositionModeCommand { get; private set; }
        public RelayCommand SetIncrementalPositionModeCommand { get; private set; }

        public RelayCommand OpenHeightMapCommand { get; private set; }
        public RelayCommand OpenGCodeFileCommand { get; private set; }
        public RelayCommand CloseFileCommand { get; private set; }

        public RelayCommand ClearHeightMapCommand { get; private set; }

        public RelayCommand ApplyHeightMapCommand { get; private set; }
        public RelayCommand ArcToLineCommand { get; private set; }
    }
}