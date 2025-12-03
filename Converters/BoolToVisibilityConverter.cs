using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DIClosedBrowserTemplate.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool visible) return null;
        if (parameter is string inverse && inverse.ToLower() == "inverse")
            return !visible ? Visibility.Visible : Visibility.Collapsed;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}