using System;
using System.Globalization;
using System.Windows.Data;
using GOT.Logic.Enums;
using GOT.SharedKernel.Enums;

namespace GOT.UI.Converters
{
    public class AutoClosingNotifyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var closingState = (ClosingState?) value;

            switch (closingState) {
                case ClosingState.Reenter:
                    return "Перезаход";
                case ClosingState.Manual:
                    return "Закрыто пользователем";
                case ClosingState.None:
                    return "";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}