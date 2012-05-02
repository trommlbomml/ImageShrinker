
using System;
using System.Windows;
using System.Windows.Data;

namespace ImageShrinker.Converters
{
    class VisibilityConverter : IValueConverter
    {
        public static VisibilityConverter Instance = new VisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value)
                {
                    return Visibility.Visible;
                }

                if (!string.IsNullOrEmpty(parameter as string) && (string)parameter == "COLLAPSED")
                    return Visibility.Collapsed;

                return Visibility.Hidden;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Visibility)
            {
                return (Visibility)value == Visibility.Visible;
            }
            return value;
        }
    }
}
