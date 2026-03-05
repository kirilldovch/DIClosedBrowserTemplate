using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DIClosedBrowserTemplate.Helpers.Behaviors;

public class DraggableBehavior : Behavior<FrameworkElement>
{
    private Point _dragStart;
    private Point _transformStart;
    private FrameworkElement? _container;

    protected override void OnAttached()
    {
        AssociatedObject.MouseLeftButtonDown += OnMouseDown;
        AssociatedObject.MouseMove += OnMouseMove;
        AssociatedObject.MouseLeftButtonUp += OnMouseUp;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseLeftButtonDown -= OnMouseDown;
        AssociatedObject.MouseMove -= OnMouseMove;
        AssociatedObject.MouseLeftButtonUp -= OnMouseUp;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _container = VisualTreeHelper.GetParent(AssociatedObject) as FrameworkElement;

        if (_container == null)
            return;

        AssociatedObject.CaptureMouse();

        if (_container.RenderTransform is not TranslateTransform)
            _container.RenderTransform = new TranslateTransform();

        var transform = (TranslateTransform)_container.RenderTransform;

        _dragStart = e.GetPosition(_container.Parent as IInputElement);
        _transformStart = new Point(transform.X, transform.Y);

        _container.Opacity = 0.6;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!AssociatedObject.IsMouseCaptured || _container == null)
            return;

        var parent = _container.Parent as FrameworkElement;
        if (parent == null)
            return;

        var transform = (TranslateTransform)_container.RenderTransform;
        var current = e.GetPosition(parent);

        var deltaX = current.X - _dragStart.X;
        var deltaY = current.Y - _dragStart.Y;

        transform.X = _transformStart.X + deltaX;
        transform.Y = _transformStart.Y + deltaY;
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        AssociatedObject.ReleaseMouseCapture();

        if (_container != null)
            _container.Opacity = 1;
    }
}