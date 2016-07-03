using System;
using System.Globalization;
using System.Windows.Data;

namespace Utils
{
    /// <summary>
    /// Converts a float number between [0,1] to a percentage
    /// </summary>
    public class PercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToSingle(value) * 100;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}