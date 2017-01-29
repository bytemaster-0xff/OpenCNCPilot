using LagoVista.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Sender.ViewModels
{
    public class ManualSendViewModel : ViewModelBase
    {

        public void PreviousCommand()
        {
            /* Idea here is to use up/down arrows to retrieve this/next commands */
            /*if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                ManualSend();
            }
            else if (e.Key == System.Windows.Input.Key.Down)
            {
                e.Handled = true;

                if (ManualCommandIndex == 0)
                {
                    TextBoxManual.Text = "";
                    ManualCommandIndex = -1;
                }
                else if (ManualCommandIndex > 0)
                {
                    ManualCommandIndex--;
                    TextBoxManual.Text = ManualCommands[ManualCommandIndex];
                    TextBoxManual.SelectionStart = TextBoxManual.Text.Length;
                }
            }
            else if (e.Key == System.Windows.Input.Key.Up)
            {
                e.Handled = true;

                if (ManualCommands.Count > ManualCommandIndex + 1)
                {
                    ManualCommandIndex++;
                    TextBoxManual.Text = ManualCommands[ManualCommandIndex];
                    TextBoxManual.SelectionStart = TextBoxManual.Text.Length;
                }
            } */
        }
    }
}
