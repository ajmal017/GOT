using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using GOT.Logic.Enums;

namespace GOT.UI.Converters
{
    public class StrategyStatesImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value) {
                case null:
                    return DependencyProperty.UnsetValue;
                case StrategyStates state:
                    switch (state) {
                        case StrategyStates.Started:
                            return "/Images/box_green_16.png";
                        case StrategyStates.ClosePositions:
                            return "/Images/box_orange_16.png";
                        case StrategyStates.Observe:
                            return "/Images/box_yellow_16.png";
                        case StrategyStates.Stopped:
                            return "/Images/box_red_16.png";
                        default:
                            return "/Images/box_red_16.png";
                    }
                case HedgeSessionStatus sessionStatus:
                    switch (sessionStatus) {
                        case HedgeSessionStatus.Started:
                            return "/Images/box_green_16.png";
                        case HedgeSessionStatus.Stopped:
                            return "/Images/box_red_16.png";
                        case HedgeSessionStatus.PartialStarted:
                            return "/Images/box_orange_16.png";
                        default:
                            return "/Images/box_red_16.png";
                    }
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}