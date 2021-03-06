﻿using LagoVista.GCode.Sender.Application.ViewModels;
using LagoVista.GCode.Sender.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LagoVista.GCode.Sender.Application.Controls
{
    /// <summary>
    /// Interaction logic for ManualSend.xaml
    /// </summary>
    public partial class ManualSend : UserControl
    {
        public ManualSend()
        {
            InitializeComponent();
        }
    
        public GCodeAppViewModelBase ViewModel
        {
            get { return DataContext as GCodeAppViewModelBase; }
        }

        private void TextBoxManual_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Up)
            {
                ViewModel.ShowPrevious();
            }
            else if (e.Key == System.Windows.Input.Key.Down)
            {
                ViewModel.ShowNext();
            }
            else if (e.Key == System.Windows.Input.Key.Return)
            {
                e.Handled = true;
                ViewModel.ManualSend();
            }
        }

        private void TextBoxManual_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TextBoxManual.CaretIndex == 0)
            {
                TextBoxManual.CaretIndex = TextBoxManual.Text.Length;
            }
        }
    }
}
