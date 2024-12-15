using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Calculater.View.Servises
{
    public class TextBoxRegExBehavior:Behavior<TextBox>
    {
        public static readonly DependencyProperty RegexProperty = DependencyProperty.Register(
            "Regex", typeof(string), typeof(TextBoxRegExBehavior));
        public string Regex {
            get { return (string)GetValue(RegexProperty); }
            set { SetValue(RegexProperty, value);}
        }

        public static readonly DependencyProperty EmptyValueProperty =
       DependencyProperty.Register("EmptyValue", typeof(string), typeof(TextBoxRegExBehavior), null);

        public string EmptyValue
        {
            get { return (string)GetValue(EmptyValueProperty); }
            set { SetValue(EmptyValueProperty, value); }
        }
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewTextInput += PreviewTextInputHandler;
            AssociatedObject.PreviewKeyDown += PreviewKeyDownHandler;
            DataObject.AddPastingHandler(AssociatedObject, PastingHandler);
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.PreviewTextInput -= PreviewTextInputHandler;
            AssociatedObject.PreviewKeyDown -= PreviewKeyDownHandler;
            DataObject.RemovePastingHandler(AssociatedObject, PastingHandler);
        }
        void PreviewTextInputHandler(object sender, TextCompositionEventArgs e)
        {
            string text;
            if (this.AssociatedObject.Text.Length < this.AssociatedObject.CaretIndex)
                text = this.AssociatedObject.Text;
            else
            {
                //  Remaining text after removing selected text.
                string remainingTextAfterRemoveSelection;

                text = TreatSelectedText(out remainingTextAfterRemoveSelection)
                    ? remainingTextAfterRemoveSelection.Insert(AssociatedObject.SelectionStart, e.Text)
                    : AssociatedObject.Text.Insert(this.AssociatedObject.CaretIndex, e.Text);
            }

            e.Handled = !ValidateText(text);
        }

        void PreviewKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(this.EmptyValue))
                return;

            string text = null;

            // Handle the Backspace key
            if (e.Key == Key.Back)
            {
                if (!this.TreatSelectedText(out text))
                {
                    if (AssociatedObject.SelectionStart > 0)
                        text = this.AssociatedObject.Text.Remove(AssociatedObject.SelectionStart - 1, 1);
                }
            }
            // Handle the Delete key
            else if (e.Key == Key.Delete)
            {
                // If text was selected, delete it
                if (!this.TreatSelectedText(out text) && this.AssociatedObject.Text.Length > AssociatedObject.SelectionStart)
                {
                    // Otherwise delete next symbol
                    text = this.AssociatedObject.Text.Remove(AssociatedObject.SelectionStart, 1);
                }
            }

            if (text == string.Empty)
            {
                this.AssociatedObject.Text = this.EmptyValue;
                if (e.Key == Key.Back)
                    AssociatedObject.SelectionStart++;
                e.Handled = true;
            }
        }

        private void PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string text = Convert.ToString(e.DataObject.GetData(DataFormats.Text));

                if (!ValidateText(text))
                    e.CancelCommand();
            }
            else
                e.CancelCommand();
        }


        /// <summary>
        ///     Validate certain text by our regular expression and text length conditions
        /// </summary>
        /// <param name="text"> Text for validation </param>
        /// <returns> True - valid, False - invalid </returns>
        private bool ValidateText(string text)
        {
            return (new Regex(this.Regex, RegexOptions.IgnoreCase)).IsMatch(text);
        }

        /// <summary>
        ///     Handle text selection
        /// </summary>
        /// <returns>true if the character was successfully removed; otherwise, false. </returns>
        private bool TreatSelectedText(out string text)
        {
            text = null;
            if (AssociatedObject.SelectionLength <= 0)
                return false;

            var length = this.AssociatedObject.Text.Length;
            if (AssociatedObject.SelectionStart >= length)
                return true;

            if (AssociatedObject.SelectionStart + AssociatedObject.SelectionLength >= length)
                AssociatedObject.SelectionLength = length - AssociatedObject.SelectionStart;

            text = this.AssociatedObject.Text.Remove(AssociatedObject.SelectionStart, AssociatedObject.SelectionLength);
            return true;
        }
    }
}
