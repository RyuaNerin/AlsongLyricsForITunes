using System;
using System.Globalization;
using System.Windows.Data;

namespace iTunesLyricOverlay.Converters
{
    class DoubleMultiplier : IValueConverter
    {
        public double Multiply { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is double v ? v * this.Multiply : throw new ArgumentException();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is double v ? v / this.Multiply : throw new ArgumentException();
    }
}
