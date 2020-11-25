using System;
using System.Globalization;
using System.Windows.Data;

namespace GOT.UI.Converters
{
    public class DescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var newDescription = "";
            try {
                if (value is string description) {
                    var index = description.IndexOf('|');
                    newDescription = description.Substring(0, index).Trim();
                    return newDescription;
                }
            }
            catch {
                newDescription = "error description";
            }

            return newDescription;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}