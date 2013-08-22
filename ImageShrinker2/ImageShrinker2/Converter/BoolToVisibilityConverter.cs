
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImageShrinker2.Converter
{
    class BoolToVisibilityConverter : IValueConverter
    {
        public static BoolToVisibilityConverter Instance = new BoolToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool ? ((bool) value) ? Visibility.Visible : Visibility.Collapsed : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
