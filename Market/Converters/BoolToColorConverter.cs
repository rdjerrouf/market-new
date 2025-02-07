// BoolToColorConverter.cs
using System.Globalization;
using Microsoft.Maui.Graphics;

namespace Market.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isRead)
            {
                return isRead ? Colors.Transparent : Colors.LightYellow;
            }
            return Colors.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

