using System;
using System.Globalization;
using System.Windows.Data;
using GOT.Logic.Enums;
using GOT.SharedKernel.Enums;

namespace GOT.UI.Converters
{
    public class ConnectionStateImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ConnectionStates? connectionState = (ConnectionStates?)value;

            switch (connectionState)
            {
                case ConnectionStates.Connected:
                    return "/Images/box_green_16.png";
                case ConnectionStates.Delay:
                    return "/Images/box_orange_16.png";
                case ConnectionStates.LossConnect:
                case ConnectionStates.Disconnected:
                    return "/Images/box_red_16.png";
                default:
                    return "/Images/box_red_16.png";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}