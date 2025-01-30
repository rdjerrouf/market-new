using System.Globalization;

namespace Market.Converters
{
    /// <summary>
    /// Converts a boolean value to its opposite value.
    /// Used primarily for inverting binding values in XAML.
    /// Example: Converting IsBusy to IsEnabled (when busy is true, enabled should be false)
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        // Converts from source property to target property
        // Example: Converting IsBusy (source) to IsEnabled (target)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return value;
        }

        // Converts back from target property to source property
        // Used when the binding is TwoWay
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return value;
        }
    }

    /// <summary>
    /// Converts a string value to a boolean based on whether the string has content.
    /// Used for showing/hiding UI elements based on string properties.
    /// Example: Show an image only when PhotoUrl has a value
    /// </summary>
    public class StringNotNullOrEmptyBoolConverter : IValueConverter
    {
        // Converts from string to boolean
        // Returns true if string has content, false if null or empty
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
                return !string.IsNullOrEmpty(stringValue);
            return false;
        }

        // Converting back not supported for this converter
        // (How would we convert true/false back to a specific string?)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}