using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CANMonitor.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue 
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28A745")) // 绿色
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC3545")); // 红色
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}