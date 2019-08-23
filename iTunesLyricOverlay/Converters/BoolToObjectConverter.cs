using System;
using System.Globalization;
using System.Windows.Data;

namespace iTunesLyricOverlay.Converters
{
    public class BoolToObjectConverter : IValueConverter
    {
        public object WhenTrue  { get; set; }
        public object WhenFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? this.WhenTrue : this.WhenFalse;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value == this.WhenTrue;
    }
}
