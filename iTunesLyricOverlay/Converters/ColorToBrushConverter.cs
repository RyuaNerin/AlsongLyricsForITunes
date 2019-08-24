using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace iTunesLyricOverlay.Converters
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is Color c ? new SolidColorBrush(c) : throw new ArgumentException();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SolidColorBrush b ? b.Color : throw new ArgumentException();
    }
}
