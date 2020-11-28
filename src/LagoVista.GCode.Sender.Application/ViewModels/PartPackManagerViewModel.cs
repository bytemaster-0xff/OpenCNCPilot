using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Application.ViewModels;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.PickAndPlace.Managers;
using LagoVista.PickAndPlace.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace LagoVista.PickAndPlace.ViewModels
{
    public class PartPackManagerViewModel : MachineVisionViewModelBase
    {
        private bool _isDirty = false;

        private string _fileName;

        private PnPMachine _pnpMachine;

        MachineVisionViewModelBase _parent;

        public PartPackManagerViewModel(IMachine machine, MachineVisionViewModelBase parent) : base(machine)
        {
            _parent = parent;

            SaveMachineCommand = new RelayCommand(SaveMachine, () => _pnpMachine != null);
            AddPartPackCommand = new RelayCommand(AddPartPack, () => _pnpMachine != null);
            OpenMachineCommand = new RelayCommand(OpenMachine);
            NewMachineCommand = new RelayCommand(NewMachine);
            DoneEditingRowCommand = new RelayCommand(DoneEditingRow, () => SelectedPartPack != null && SelectedPartPack.SelectedRow != null);

            AddSlotCommand = new RelayCommand(AddSlot);
            DoneEditSlotCommand = new RelayCommand(() => SelectedSlot = null);

            SetSlotXCommand = new RelayCommand(SetSlotX);
            SetSlotYCommand = new RelayCommand(SetSlotY);
            SetPartPackXCommand = new RelayCommand(() => SelectedPartPack.Pin1XOffset = Machine.MachinePosition.X);
            SetPartPackXCommand = new RelayCommand(() => SelectedPartPack.Pin1YOffset = Machine.MachinePosition.Y);
            GoToSlotCommand = new RelayCommand(GoToSlot);
            SetPin1InFeederCommand = new RelayCommand(SetPin1InFeeder);
            GoToPin1InFeederCommand = new RelayCommand(GoToPin1InFeeder);
            FindPin1InFeederCommand = new RelayCommand(FindPin1InFeeder);
            GoToCurrentPartCommand = new RelayCommand(GoToCurrentPart);
        }

        public PartPackManagerViewModel(IMachine machine)  : this(machine, null)
        {

        }

        public async void OpenMachine()
        {
            if (SelectedPartPack != null || _isDirty)
            {
                if (!await Popups.ConfirmAsync("Lose Changes?", "You have unsaved work, opening a new file will cause you to lose changes.\r\n\r\nContinue?"))
                {
                    return;
                }
            }

            _fileName = await Popups.ShowOpenFileAsync("PnP Machine (*.pnp)|*.pnp");
            if (!String.IsNullOrEmpty(_fileName))
            {
                var machine = await PnPMachineManager.GetPnPMachineAsync(_fileName);
                SetMachine(machine);
            }
        }

        public void GoToPin1InFeeder()
        {
            if (SelectedPartPack != null)
            {
                var slot = _pnpMachine.Carrier.PartPackSlots.Where(pps => pps.PartPack.Id == SelectedPartPack.Id).FirstOrDefault();

                Machine.GotoPoint(SelectedPartPack.Pin1XOffset + slot.X, SelectedPartPack.Pin1YOffset + slot.Y);
            }
        }

        public void SetPin1InFeeder()
        {
            if (SelectedPartPack != null)
            {
                var slot = _pnpMachine.Carrier.PartPackSlots.Where(pps => pps.PartPack.Id == SelectedPartPack.Id).FirstOrDefault();

                SelectedPartPack.Pin1XOffset = Machine.MachinePosition.X - slot.X;
                SelectedPartPack.Pin1YOffset = Machine.MachinePosition.Y - slot.Y;
            }
        }

        public void FindPin1InFeeder()
        {
            if (SelectedPartPack != null)
            {
                if (_parent != null)
                {
                    _parent.SelectMVProfile("tapehole");
                    _parent.ShowCircles = true;
                }

                var slot = _pnpMachine.Carrier.PartPackSlots.Where(pps => pps.PartPack.Id == SelectedPartPack.Id).FirstOrDefault();
                var x = slot.X + SelectedPartPack.Pin1XOffset + 4;
                var y = slot.Y + SelectedPartPack.Pin1YOffset;
                Machine.GotoPoint(x, y);
                IsLocating = true;
            }
        }

        public void FoundLocation(Point2D<double> point, double diameter)
        {
            Debug.WriteLine(point + " " + diameter);
            IsLocating = false;

            var slot = _pnpMachine.Carrier.PartPackSlots.Where(pps => pps.PartPack.Id == SelectedPartPack.Id).FirstOrDefault();

            SelectedPartPack.Pin1XOffset = (Machine.MachinePosition.X - 4) - slot.X;
            SelectedPartPack.Pin1YOffset = Machine.MachinePosition.Y - slot.Y;
            var x = slot.X + SelectedPartPack.Pin1XOffset;
            var y = slot.Y + SelectedPartPack.Pin1YOffset;
            

            Machine.GotoPoint(x, y);
            IsLocating = false;

        }

        public void GoToCurrentPart()
        {
            if (CurrentPartX.HasValue && CurrentPartY.HasValue)
            {
                Machine.GotoPoint(CurrentPartX.Value, CurrentPartY.Value);
            }
        }

        public void SetMachine(PnPMachine machine)
        {
            _pnpMachine = machine;
            SaveMachineCommand.RaiseCanExecuteChanged();
            AddPartPackCommand.RaiseCanExecuteChanged();

            RaisePropertyChanged(nameof(PartPacks));
            RaisePropertyChanged(nameof(Slots));
        }

        public async void NewMachine()
        {
            if (_isDirty)
            {
                if (!await Popups.ConfirmAsync("Lose Changes?", "You have unsaved work, opening a new file will cause you to lose changes.\r\n\r\nContinue?"))
                {
                    return;
                }
            }

            _fileName = null;
            _pnpMachine = new PnPMachine();

            SaveMachineCommand.RaiseCanExecuteChanged();
            AddPartPackCommand.RaiseCanExecuteChanged();

            RaisePropertyChanged(nameof(PartPacks));
            RaisePropertyChanged(nameof(Slots));
        }

        public void GoToSlot()
        {
            if (SelectedSlot != null)
            {
                Machine.GotoPoint(SelectedSlot.X, SelectedSlot.Y);
            }
        }

        public void SetSlotX()
        {
            if (SelectedSlot != null)
            {
                SelectedSlot.X = Machine.MachinePosition.X;
            }
        }

        public void SetSlotY()
        {
            if (SelectedSlot != null)
            {
                SelectedSlot.Y = Machine.MachinePosition.Y;
            }
        }

        public void DoneEditingRow()
        {
            if (SelectedPartPack != null)
            {
                SelectedPartPack.SelectedRow = null;
            }
        }


        public void AddPartPack()
        {
            if (_pnpMachine != null)
            {
                var newPartPack = new PartPackFeeder()
                {
                    Name = $"Pack {_pnpMachine.Carrier.AvailablePartPacks.Count + 1}",
                    Id = $"pack{_pnpMachine.Carrier.AvailablePartPacks.Count + 1}",
                };

                _pnpMachine.Carrier.AvailablePartPacks.Add(newPartPack);

                SelectedPartPack = newPartPack;
                _isDirty = true;
            }
        }

        public void AddSlot()
        {
            if (_pnpMachine != null)
            {
                var slot = new PartPackSlot()
                {
                    Width = 70,
                    Height = 70
                };

                _pnpMachine.Carrier.PartPackSlots.Add(slot);
                SelectedSlot = slot;
            }
        }

        public async void SaveMachine()
        {
            var json = JsonConvert.SerializeObject(_pnpMachine);
            if (String.IsNullOrEmpty(_fileName))
            {
                _fileName = await Popups.ShowSaveFileAsync("PnP Machine (*.pnp)|*.pnp");
                if (String.IsNullOrEmpty(_fileName))
                {
                    return;
                }
            }

            await Storage.WriteAllTextAsync(_fileName, json);
            _isDirty = false;
        }

        public ObservableCollection<PartPackSlot> Slots
        {
            get
            {
                if (_pnpMachine == null)
                    return null;

                return new ObservableCollection<PartPackSlot>(_pnpMachine?.Carrier.PartPackSlots.OrderBy(slt => slt.Row).ThenBy(slt => slt.Column));
            }
        }

        public ObservableCollection<PartPackFeeder> PartPacks
        {
            get => _pnpMachine?.Carrier.AvailablePartPacks;
        }

        private PartPackFeeder _selectedPartPack = null;
        public PartPackFeeder SelectedPartPack
        {
            get => _selectedPartPack;
            set
            {
                if (_selectedPartPack != null)
                {
                    _selectedPartPack.PropertyChanged -= _selectedPartPack_PropertyChanged;
                }

                Set(ref _selectedPartPack, value);
                if (value != null)
                {
                    SelectedSlot = _pnpMachine.Carrier.PartPackSlots.SingleOrDefault(slt => slt.PartPack != null && slt.PartPack.Id == value.Id);
                    _selectedPartPack.PropertyChanged += _selectedPartPack_PropertyChanged;
                }
                else
                {
                    SelectedSlot = null;
                }
            }
        }

        private void _selectedPartPack_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PartPackFeeder.SelectedRow))
            {
                RaisePropertyChanged(nameof(SelectedRow));
                RaisePropertyChanged(nameof(SelectedPart));
                RaisePropertyChanged(nameof(CurrentPackage));
                RaisePropertyChanged(nameof(CurrentPartX));
                RaisePropertyChanged(nameof(CurrentPartY));
            }
        }

        public Part SelectedPart
        {
            get => SelectedRow?.Part;
        }

        public Row SelectedRow
        {
            get => SelectedPartPack?.SelectedRow;
        }

        private PartPackSlot _selectedSlot;
        public PartPackSlot SelectedSlot
        {
            get => _selectedSlot;
            set
            {
                Set(ref _selectedSlot, value);
                RaisePropertyChanged(nameof(CurrentSlotPartPack));
            }
        }

        public Package CurrentPackage
        {
            get
            {
                if (SelectedPart != null)
                {
                    return Packages.SingleOrDefault(pck => pck.Name == SelectedPart.PackageName);
                }

                return null;
            }
        }

        public double? CurrentPartX
        {
            get
            {
                if (SelectedPartPack != null && SelectedRow != null && SelectedPart != null && CurrentPackage != null)
                {
                    return SelectedSlot.X + SelectedPartPack.Pin1XOffset + CurrentPackage.CenterXFromHole;
                }

                return null;
            }
        }

        public double? CurrentPartY
        {
            get
            {
                if (SelectedPartPack != null && SelectedRow != null && SelectedPart != null && CurrentPackage != null)
                {
                    return SelectedSlot.Y + SelectedPartPack.Pin1YOffset + CurrentPackage.CenterYFromHole + (SelectedRow.RowNumber - 1) * SelectedPartPack.RowHeight;
                }

                return null;
            }
        }


        public string CurrentSlotPartPack
        {
            get => SelectedSlot?.PartPack?.Id;
            set
            {
                if (SelectedSlot != null && !String.IsNullOrEmpty(value))
                {
                    SelectedSlot.PartPack = EntityHeader.Create(value, PartPacks.First(pp => pp.Id == value).Name);
                }
                else
                {
                    SelectedPartPack = null;
                }
            }
        }

        public IEnumerable<Package> Packages { get => _pnpMachine?.Packages; }

        public RelayCommand SaveMachineCommand { get; }
        public RelayCommand OpenMachineCommand { get; }
        public RelayCommand NewMachineCommand { get; }
        public RelayCommand AddPartPackCommand { get; }
        public RelayCommand DoneEditingRowCommand { get; }


        public RelayCommand SetPartPackXCommand { get; }
        public RelayCommand SetPartPackYCommand { get; }

        public RelayCommand DoneEditSlotCommand { get; }
        public RelayCommand AddSlotCommand { get; }

        public RelayCommand SetSlotXCommand { get; }
        public RelayCommand SetSlotYCommand { get; }

        public RelayCommand GoToCurrentPartCommand { get; }

        public RelayCommand GoToSlotCommand { get; }

        public RelayCommand GoToPin1InFeederCommand { get; }       
        public RelayCommand SetPin1InFeederCommand { get; }
        public RelayCommand FindPin1InFeederCommand { get; }

        public bool IsLocating { get; private set; }
    }
}
