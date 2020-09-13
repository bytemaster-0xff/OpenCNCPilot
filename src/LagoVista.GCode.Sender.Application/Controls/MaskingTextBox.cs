using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LagoVista.GCode.Sender.Application.Controls
{
    public class MaskingTextBox : TextBox
    {
        System.Text.RegularExpressions.Regex _regEx;

        public MaskingTextBox()
        {
            PreviewKeyDown += MaskingTextBox_PreviewKeyDown;
            PreviewTextInput += MaskingTextBox_PreviewTextInput;
        }

        private void MaskingTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (_regEx != null)
            {
                var newText = GetProposedText(sender as TextBox, e.Text);
                if (!_regEx.IsMatch(newText))
                {
                    e.Handled = true;
                }

                switch (UnitType)
                {

                    /* For now it's all...if we want text we wont want to do min/max compare */
                    case UnitTypes.Count:
                    case UnitTypes.RPM:
                    case UnitTypes.Rate:
                    case UnitTypes.Size:
                    case UnitTypes.Seconds:
                        double value;
                        if (Double.TryParse(newText, out value))
                        {
                            if(Min.HasValue)
                            {
                                if (value < Min.Value)
                                    e.Handled = true;
                            }

                            if(Max.HasValue)
                            {
                                if (value > Max.Value)
                                    e.Handled = true;
                            }
                        }
                        else
                        {
                            /* Really should never get here...*/
                            e.Handled = true;
                        }

                        break;
                }
            }
        }

        private static string GetProposedTextBackspace(TextBox textBox)
        {
            var text = GetTextWithSelectionRemoved(textBox);
            if (textBox.SelectionStart > 0 && textBox.SelectionLength == 0)
            {
                text = text.Remove(textBox.SelectionStart - 1, 1);
            }

            return text;
        }


        private static string GetProposedText(TextBox textBox, string newText)
        {
            var text = GetTextWithSelectionRemoved(textBox);
            text = text.Insert(textBox.CaretIndex, newText);

            return text;
        }

        private static string GetTextWithSelectionRemoved(TextBox textBox)
        {
            var text = textBox.Text;

            if (textBox.SelectionStart != -1)
            {
                text = text.Remove(textBox.SelectionStart, textBox.SelectionLength);
            }
            return text;
        }

        private void MaskingTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Space)
            {
                e.Handled = true;
            }
        }

        public enum UnitTypes
        {
            Count,
            Size,
            RPM,
            Rate,
            Seconds,
            String
        }

        private UnitTypes _unitType;
        public UnitTypes UnitType
        {
            get { return _unitType; }
            set
            {
                _unitType = value;
                TextAlignment = System.Windows.TextAlignment.Right;
                switch (value)
                {
                    case UnitTypes.Count:
                    case UnitTypes.Rate:
                    case UnitTypes.RPM:
                    case UnitTypes.Seconds:
                        _regEx = new System.Text.RegularExpressions.Regex(@"^\d+$");
                        break;
                    case UnitTypes.Size:
                        _regEx = new System.Text.RegularExpressions.Regex(@"^\d+(\.)?(\.\d{1,4})?$");
                        break;
                }
            }
        }

        public double? Min { get; set; }
        public double? Max { get; set; }

    }
}
