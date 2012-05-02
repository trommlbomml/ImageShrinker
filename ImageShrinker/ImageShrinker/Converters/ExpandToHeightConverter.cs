using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImageShrinker.Converters
{
    public class ExpandToHeightConverter : IValueConverter
    {
        public static ExpandToHeightConverter Instance = new ExpandToHeightConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool) value ? new GridLength(1, GridUnitType.Star) : GridLength.Auto;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
