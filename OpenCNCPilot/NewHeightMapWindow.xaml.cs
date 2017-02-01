using LagoVista.Core.Models.Drawing;
using System;
using System.Windows;

namespace LagoVista.GCode.Sender.Application
{
	public partial class NewHeightMapWindow : Window
	{
		public NewHeightMapWindow(IMachine machine, Models.HeightMap heightMap)
		{
            var vm = new ViewModels.NewHeightMapViewModel(machine);
            vm.HeightMap = heightMap;
            DataContext = vm;

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
