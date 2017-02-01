using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Models;
using LagoVista.GCode.Sender.ViewModels;
using System;
using System.Windows;

namespace LagoVista.GCode.Sender.Application
{
	public partial class NewHeightMapWindow : Window
	{
		public NewHeightMapWindow(Window owner, IMachine machine)
		{
            var vm = new NewHeightMapViewModel(machine);
            vm.HeightMap = new HeightMap();
            Owner = owner;            

            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            DataContext = vm;

            InitializeComponent();
        }

        public NewHeightMapViewModel ViewModel
        {
            get { return DataContext as NewHeightMapViewModel; }
        }

        public HeightMap HeightMap
        {
            get { return ViewModel.HeightMap; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
