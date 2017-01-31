using LagoVista.Core.Models.Drawing;
using System;
using System.Windows;

namespace LagoVista.GCode.Sender.Application
{
	public partial class NewHeightMapWindow : Window
	{
		public NewHeightMapWindow(Models.HeightMap heightMap)
		{
			InitializeComponent();
            var vm =  new ViewModels.NewHeightMapViewModel(App.Current.Machine, App.Current.Settings);
            vm.HeightMap = heightMap;
            DataContext = vm;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
