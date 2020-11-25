using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GOT.UI.Converters
{
    public class PnlColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = new SolidColorBrush(Colors.AliceBlue);
            if (value == null)
                return brush;
            return value.ToString().Contains("-")
                ? new SolidColorBrush(Colors.Red)
                : new SolidColorBrush(Colors.ForestGreen);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}