using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CANMonitor.Converters
{
    public class StringToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && parameter is string parameterValue)
            {
                return string.Equals(stringValue, parameterValue, StringComparison.OrdinalIgnoreCase) 
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A90E2")) 
                    : new SolidColorBrush(Colors.White);
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}