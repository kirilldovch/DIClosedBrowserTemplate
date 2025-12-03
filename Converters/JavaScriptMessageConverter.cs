using System.Globalization;
using System.Windows.Data;
using CefSharp;

namespace DIClosedBrowserTemplate.Converters;

public class JavaScriptMessageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value as JavascriptMessageReceivedEventArgs;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}