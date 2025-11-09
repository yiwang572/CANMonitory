using System;
using System.Globalization;
using System.Windows.Data;

namespace CANMonitor.Converters
{
    public class StringToUshortConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ushort ushortValue)
            {
                return ushortValue.ToString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && ushort.TryParse(stringValue, out ushort result))
            {
                return result;
            }
            return 0;
        }
    }
}