using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using DIClosedBrowserTemplate.Helpers;

namespace DIClosedBrowserTemplate.Converters
{
    public class ImageConverter : MarkupExtension, IValueConverter
    {
        public int? Width { get; set; }
        public int? Height { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is not Uri path
                ? null
                : ImageHelper.GetImage(path, Width, Height);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
