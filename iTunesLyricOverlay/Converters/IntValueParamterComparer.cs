using System;
using System.Globalization;
using System.Windows.Data;

namespace iTunesLyricOverlay.Converters
{
    public class IntValueParamterComparer : IValueConverter
    {
        public object WhenTrue  { get; set; }
        public object WhenFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => System.Convert.ToInt32(value) == System.Convert.ToInt32(parameter) ? this.WhenTrue: this.WhenFalse;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
