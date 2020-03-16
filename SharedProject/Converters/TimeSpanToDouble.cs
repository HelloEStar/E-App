using System;
using System.Globalization;
using System.Windows.Data;

namespace SharedProject
{
    /// <summary>
    /// 时间间隔-浮点 转换器
    /// </summary>
    public class TimeSpanToDouble : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((TimeSpan)value).TotalSeconds;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TimeSpan.FromSeconds((double)value);
        }
    }
}
