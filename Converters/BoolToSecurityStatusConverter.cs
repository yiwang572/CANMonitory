using System;
using System.Globalization;
using System.Windows.Data;

namespace CANMonitor.Converters
{
    public class BoolToSecurityStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "已解锁" : "未解锁";
            }
            return "未解锁";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}