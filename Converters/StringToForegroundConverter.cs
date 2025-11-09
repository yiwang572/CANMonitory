using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CANMonitor.Converters
{
    public class StringToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && parameter is string parameterValue)
            {
                return string.Equals(stringValue, parameterValue, StringComparison.OrdinalIgnoreCase) 
                    ? new SolidColorBrush(Colors.White) 
                    : new SolidColorBrush(Colors.Black);
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}