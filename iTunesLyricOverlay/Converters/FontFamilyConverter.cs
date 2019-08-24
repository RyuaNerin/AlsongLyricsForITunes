using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace iTunesLyricOverlay.Converters
{
    public class FontFamilyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is string name ? new FontFamily(name) : throw new ArgumentException();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is FontFamily ff ? ff.Source : throw new ArgumentException();
    }
}
