using System;
using System.Globalization;
using System.Windows.Data;

namespace CANMonitor.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "已启用" : "已禁用";
            }
            return "未知";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return string.Equals(stringValue, "已启用", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}