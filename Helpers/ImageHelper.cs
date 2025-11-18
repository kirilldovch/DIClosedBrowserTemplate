using System.Windows.Media.Imaging;

namespace DIClosedBrowserTemplate.Helpers
{
    public class ImageHelper
    {
        public static BitmapImage GetImage(Uri path, int? width = null, int? height = null)
        {
            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.None;

            if (width.HasValue)
                bitmap.DecodePixelWidth = width.Value;

            if (height.HasValue)
                bitmap.DecodePixelHeight = height.Value;

            bitmap.UriSource = path;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
    }
}
