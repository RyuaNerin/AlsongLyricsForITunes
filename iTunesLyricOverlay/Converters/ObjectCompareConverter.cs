using System;
using System.Globalization;
using System.Windows.Data;

namespace iTunesLyricOverlay.Converters
{
    public class ObjectCompareConverter : IMultiValueConverter
    {
        public object OnTrue { get; set; }
        public object OnFalse { get; set; }
        
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            for (int i = 0; i < values.Length - 1; ++i)
                if (values[i] != values[i + 1])
                    return this.OnFalse;

            return this.OnTrue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
