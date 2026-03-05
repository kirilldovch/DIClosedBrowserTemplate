using System.Globalization;
using System.Windows.Data;
using DIClosedBrowserTemplate.Utilities;

namespace DIClosedBrowserTemplate.Converters;

public class ColorInvertConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? new InvertEffect() : null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}