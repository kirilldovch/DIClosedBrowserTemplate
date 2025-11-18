using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DIClosedBrowserTemplate.Converters;

public class Resolution4KConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;

        var param = parameter as string;

        return param switch
        {
            "Width" => screenWidth > screenHeight ? 3840 : 2160,
            "Height" => screenHeight > screenWidth ? 3840 : 2160,
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}