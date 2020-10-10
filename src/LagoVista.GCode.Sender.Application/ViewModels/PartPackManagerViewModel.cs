using HelixToolkit.Wpf;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.ViewModels;
using LagoVista.GCode.Sender.Interfaces;
using LagoVista.GCode.Sender.ViewModels;
using LagoVista.PickAndPlace.Managers;
using LagoVista.PickAndPlace.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels
{
    public class PartPackManagerViewModel : GCodeAppViewModelBase
    {
        private bool _isDirty = false;

        private string _fileName;

        private PnPMachine _machine;

        public PartPackManagerViewModel(IMachine machine) : base(machine)
        {
            SaveMachineCommand = new RelayCommand(SaveMachine, () => _machine != null);
            AddPartPackCommand = new RelayCommand(AddPartPack, () => _machine != null);
            OpenMachineCommand = new RelayCommand(OpenMachine);
            NewMachineCommand = new RelayCommand(NewMachine);
            DoneEditingRowCommand = new RelayCommand(DoneEditingRow, () => SelectedPartPack != null && SelectedPartPack.SelectedRow != null);

            AddSlotCommand = new RelayCommand(AddSlot);
            DoneEditSlotCommand = new RelayCommand(() => SelectedSlot = null);

            SetSlotXCommand = new RelayCommand(SetSlotX);
            SetSlotYCommand = new RelayCommand(SetSlotY);
            GoToSlotCommand = new RelayCommand(GoToSlot);
            GoToPin1InFeederCommand = new RelayCommand(GoToPin1InFeeder);
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
            if(SelectedPartPack != null)
            {
                var slot = _machine.Carrier.PartPackSlots.Where(pps => pps.PartPack.Id == SelectedPartPack.Id).FirstOrDefault();
                var x = slot.X + SelectedPartPack.Pin1XOffset;
                var y = slot.Y + SelectedPartPack.Pin1YOffset;
                Machine.GotoPoint(x, y);
            }
        }

        public void SetMachine(PnPMachine machine)
        {
            _machine = machine;
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
            _machine = new PnPMachine();

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
            if (_machine != null)
            {
                var newPartPack = new PartPackFeeder()
                {
                    Name = $"Pack {_machine.Carrier.AvailablePartPacks.Count + 1}",
                    Id = $"pack{_machine.Carrier.AvailablePartPacks.Count + 1}",
                };

                _machine.Carrier.AvailablePartPacks.Add(newPartPack);

                SelectedPartPack = newPartPack;
                _isDirty = true;
            }
        }

        public void AddSlot()
        {
            if (_machine != null)
            {
                var slot = new PartPackSlot()
                {
                    Width = 70,
                    Height = 70
                };

                _machine.Carrier.PartPackSlots.Add(slot);
                SelectedSlot = slot;
            }
        }

        public async void SaveMachine()
        {
            var json = JsonConvert.SerializeObject(_machine);
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
            get => new ObservableCollection<PartPackSlot>(_machine?.Carrier.PartPackSlots.OrderBy(slt => slt.Row).ThenBy(slt => slt.Column));
        }

        public ObservableCollection<PartPackFeeder> PartPacks
        {
            get => _machine?.Carrier.AvailablePartPacks;
        }

        private PartPackFeeder _selectedPartPack = null;
        public PartPackFeeder SelectedPartPack
        {
            get => _selectedPartPack;
            set
            {
                if(_selectedPartPack != null)
                {
                    _selectedPartPack.PropertyChanged -= _selectedPartPack_PropertyChanged;
                }

                Set(ref _selectedPartPack, value);
                if (value != null)
                {
                    SelectedSlot = _machine.Carrier.PartPackSlots.SingleOrDefault(slt => slt.PartPack != null && slt.PartPack.Id == value.Id);
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
            if(e.PropertyName == nameof(PartPackFeeder.SelectedRow))
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
                if(SelectedPart != null)
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
                if(SelectedPartPack != null && SelectedRow != null && SelectedPart != null && CurrentPackage != null)
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

        public IEnumerable<Package> Packages { get => _machine?.Packages; }

        public RelayCommand SaveMachineCommand { get; }
        public RelayCommand OpenMachineCommand { get; }
        public RelayCommand NewMachineCommand { get; }
        public RelayCommand AddPartPackCommand { get; }
        public RelayCommand DoneEditingRowCommand { get; }


        public RelayCommand DoneEditSlotCommand { get; }
        public RelayCommand AddSlotCommand { get; }

        public RelayCommand SetSlotXCommand { get; }
        public RelayCommand SetSlotYCommand { get; }

        public RelayCommand GoToSlotCommand { get; }

        public RelayCommand GoToPin1InFeederCommand { get; }
    }
}
