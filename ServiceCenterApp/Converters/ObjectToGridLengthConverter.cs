using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ServiceCenterApp.Converters
{
    public class ObjectToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new GridLength(1, GridUnitType.Star);
            }

            if (double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double width))
            {
                return new GridLength(width);
            }

            return new GridLength(1, GridUnitType.Auto);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}