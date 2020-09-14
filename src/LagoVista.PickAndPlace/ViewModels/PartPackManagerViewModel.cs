using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
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
    public class PartPackManagerViewModel : ViewModelBase
    {
        private bool _isDirty = false;

        private string _fileName;

        private PnPMachine _machine;

        public PartPackManagerViewModel()
        {
            SaveMachineCommand = new RelayCommand(SaveMachine, () => _machine != null);
            AddPartPackCommand = new RelayCommand(AddPartPack, () => _machine != null);
            OpenMachineCommand = new RelayCommand(OpenMachine);
            NewMachineCommand = new RelayCommand(NewMachine);
            DoneEditingRowCommand = new RelayCommand(DoneEditingRow, () => SelectedPartPack != null && SelectedPartPack.SelectedRow != null);

            AddSlotCommand = new RelayCommand(AddSlot);
            DoneEditSlotCommand = new RelayCommand(() => SelectedSlot = null);
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
            get => _machine?.Carrier.PartPackSlots;
        }

        public ObservableCollection<PartPackFeeder> PartPacks
        {
            get => _machine?.Carrier.AvailablePartPacks;
        }

        private PartPackFeeder _selectedPartPack = null;
        public PartPackFeeder SelectedPartPack
        {
            get => _selectedPartPack;
            set => Set(ref _selectedPartPack, value);
        }

        private PartPackSlot _selectedSlot;
        public PartPackSlot SelectedSlot
        {
            get => _selectedSlot;
            set => Set(ref _selectedSlot, value);
        }

        public IEnumerable<Package> Packages { get => _machine?.Packages; }

        public RelayCommand SaveMachineCommand { get; }
        public RelayCommand OpenMachineCommand { get; }
        public RelayCommand NewMachineCommand { get; }
        public RelayCommand AddPartPackCommand { get; }
        public RelayCommand DoneEditingRowCommand { get; }


        public RelayCommand DoneEditSlotCommand { get; }
        public RelayCommand AddSlotCommand { get; }
    }
}
