using System.Windows;
using System.Windows.Controls;

namespace DIClosedBrowserTemplate.Views.Controls;

public class AdaptiveViewBox : Viewbox
{
    public static readonly DependencyProperty IsScaledProperty = DependencyProperty.Register(nameof(IsScaled),
        typeof(bool), typeof(AdaptiveViewBox), new PropertyMetadata(false, OnIsScaledChanged));

    public bool IsScaled
    {
        get => (bool)GetValue(IsScaledProperty);
        set => SetValue(IsScaledProperty, value);
    }

    static AdaptiveViewBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AdaptiveViewBox),
            new FrameworkPropertyMetadata(typeof(AdaptiveViewBox)));
    }

    private static async void OnIsScaledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AdaptiveViewBox box)
            return;

        var isScaled = (bool)e.NewValue;

        const int steps = 40;
        double targetWidth;
        double targetHeight;

        if (isScaled)
        {
            targetWidth = box.Width / 2;
            targetHeight = box.Height / 2;

            var deltaW = box.Width - targetWidth;
            var deltaH = box.Height - targetHeight;

            var stepW = deltaW / 40;
            var stepH = deltaH / 40;

            for (var i = 0; i < steps; i++)
            {
                box.Width -= stepW;
                box.Height -= stepH;
                await Task.Delay(3);
            }
        }
        else
        {
            targetWidth = box.Width * 2;
            targetHeight = box.Height * 2;

            var deltaW = Math.Abs(box.Width - targetWidth);
            var deltaH = Math.Abs(box.Height - targetHeight);

            var stepW = deltaW / 40;
            var stepH = deltaH / 40;

            for (var i = 0; i < steps; i++)
            {
                box.Width += stepW;
                box.Height += stepH;
                await Task.Delay(3);
            }
        }
    }
}