using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace LagoVista.GCode.Sender.Application.Controls
{
    /// <summary>
    /// Interaction logic for CurrentFileStatus.xaml
    /// </summary>
    public partial class CurrentFileControl : UserControl
    {
        public CurrentFileControl()
        {
            InitializeComponent();
        }

        public void NewFileLoaded()
        {
            LabelFileLength.Content = App.Current.Machine.File.Count;

            int digits = (int)Math.Ceiling(Math.Log10(App.Current.Machine.File.Count));

            string format = "D" + digits;

            int i = 1;

            ListViewFile.Items.Clear();
            foreach (string line in App.Current.Machine.File)
            {
                ListViewFile.Items.Add(new TextBlock() { Text = $"{i++.ToString(format)} : {line}" });
            }

        }

        public void RefreshFileSendStatus()
        {
            LabelFilePosition.Content = App.Current.Machine.FilePosition;

            if (ListViewFile.SelectedItem is TextBlock)
                ((TextBlock)ListViewFile.SelectedItem).Background = Brushes.Transparent;

            ListViewFile.SelectedIndex = App.Current.Machine.FilePosition;

            if (ListViewFile.SelectedItem is TextBlock)
                ((TextBlock)ListViewFile.SelectedItem).Background = Brushes.Gray;

            ListViewFile.ScrollIntoView(ListViewFile.SelectedItem);
        }
    }
}
