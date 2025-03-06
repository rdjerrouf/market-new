using System;
using System.Globalization;

namespace Market.Converters
{
    /// <summary>
    /// Converter that returns true if a value is greater than zero
    /// </summary>
    public class GreaterThanZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
                return intValue > 0;
            if (value is double doubleValue)
                return doubleValue > 0;
            if (value is float floatValue)
                return floatValue > 0;
            if (value is long longValue)
                return longValue > 0;

            // Handle nullable int
            // Alternative approach
            if (value != null && value.GetType() == typeof(int?))
            {
                int? nullableInt = (int?)value;
                return nullableInt.HasValue && nullableInt.Value > 0;
            }
            // Default case - return false for any other type
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}