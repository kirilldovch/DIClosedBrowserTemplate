using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DIClosedBrowserTemplate.Converters;

public class ResolutionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        (parameter as string) == "Width" ? SystemParameters.PrimaryScreenWidth : SystemParameters.PrimaryScreenHeight;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}