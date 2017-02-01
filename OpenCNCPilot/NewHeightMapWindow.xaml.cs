using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.ViewModels;
using System;
using System.Windows;

namespace LagoVista.GCode.Sender.Application
{
	public partial class NewHeightMapWindow : Window
	{
		public NewHeightMapWindow(Window owner, IMachine machine, Models.HeightMap heightMap)
		{
            var vm = new NewHeightMapViewModel(machine);
            vm.HeightMap = heightMap;
            Owner = owner;            

            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            DataContext = vm;

            InitializeComponent();
        }

        public NewHeightMapViewModel ViewModel
        {
            get { return DataContext as NewHeightMapViewModel; }
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
