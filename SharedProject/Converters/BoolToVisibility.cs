using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SharedProject
{
    /// <summary>
    /// 可见性-布尔 转换器
    /// </summary>
    public class BoolToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Visibility)value) == Visibility.Visible;
        }
    }
}
