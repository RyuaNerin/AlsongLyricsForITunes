using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace iTunesLyricOverlay.Converters
{
    public class LyricStateToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (LyricState)value == (LyricState)parameter ? Visibility.Visible : Visibility.Hidden;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => null;
    }
}
