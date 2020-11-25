using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace GOT.UI.Views
{
    public class FrameWindow : Window, INotifyPropertyChanged
    {
        protected virtual void PreviewTextInputHandler(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsTextAllowed(e.Text);
        }

        protected void PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string))) {
                var text = (string) e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text)) {
                    e.CancelCommand();
                }
            } else {
                e.CancelCommand();
            }
        }

        private static bool IsTextAllowed(string text)
        {
            var regex = new Regex("[^0-9.,-]+");
            return regex.IsMatch(text);
        }

        #region INotifyPropertyChanged releases

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}