using System;
using System.Globalization;
using System.Windows.Data;

namespace iTunesLyricOverlay.Converters
{
    public class PlayerPositionVisualizer : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var cur = (int)values[0] / 1000;
            var max = (int)values[1] / 1000;

            return string.Format("{0:00}:{1:00} / {2:00}:{3:00}", cur / 60, cur % 60, max / 60, max % 60);
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => null;
    }
}
