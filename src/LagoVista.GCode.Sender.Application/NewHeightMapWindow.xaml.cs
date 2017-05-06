using LagoVista.GCode.Sender.Interfaces;
using LagoVista.GCode.Sender.Models;
using LagoVista.GCode.Sender.ViewModels;
using System.Windows;

namespace LagoVista.GCode.Sender.Application
{
	public partial class NewHeightMapWindow : Window
	{
        NewHeightMapViewModel _viewModel;
        public NewHeightMapWindow(Window owner, IMachine machine, bool edit)
		{
            Owner = owner;
            _viewModel = new NewHeightMapViewModel(machine);
            _viewModel.HeightMap = edit ? _viewModel.Machine.HeightMapManager.HeightMap : new HeightMap(_viewModel.Machine, _viewModel.Logger);
            ///TODO: Should really disable the edit option if we don't have a height map.
            if (_viewModel.HeightMap == null)
            {
                _viewModel.HeightMap = new HeightMap(_viewModel.Machine, _viewModel.Logger);
            }

            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            DataContext = _viewModel;

            InitializeComponent();
        }

        public HeightMap HeightMap
        {
            get { return _viewModel.HeightMap; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
