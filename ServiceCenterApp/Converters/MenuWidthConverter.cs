using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ServiceCenterApp.Converters
{
    public class MenuWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || !(values[0] is bool) || !(values[1] is double))
            {
                return new GridLength(0);
            }
            try
            {
                bool isVisible = (bool)values[0];
                double animatedWidth = (double)values[1];
                double finalWidth = isVisible ? animatedWidth : 0;
                return new GridLength(finalWidth);
            }
            catch (Exception ex)
            {
                return new GridLength(0);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}