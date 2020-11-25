using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GOT.UI.Converters
{
    public class SignConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) {
                return DependencyProperty.UnsetValue;
            }

            var decValue = decimal.Parse(value.ToString());

            if (decValue > 0) {
                return "+" + decValue;
            }

            return decValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}