using System.Windows;
using System.Windows.Input;

namespace DIClosedBrowserTemplate.Helpers
{
    public class ManipulationBoundaryFeedbackHelper
    {
        public static readonly DependencyProperty HandledProperty = DependencyProperty.RegisterAttached(
            "Handled", typeof(bool), typeof(ManipulationBoundaryFeedbackHelper), new PropertyMetadata(default(bool), HandledPropertyChangedCallback));

        public static void SetHandled(DependencyObject element, bool value)
        {
            element.SetValue(HandledProperty, value);
        }

        public static bool GetHandled(DependencyObject element)
        {
            return (bool)element.GetValue(HandledProperty);
        }

        private static void HandledPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FrameworkElement element) ||
                !(e.OldValue is bool oldValue) ||
                !(e.NewValue is bool newValue))
                return;

            if (oldValue && !newValue)
            {
                element.ManipulationBoundaryFeedback -= ElementOnManipulationBoundaryFeedback;
            }
            if (!oldValue && newValue)
            {
                element.ManipulationBoundaryFeedback += ElementOnManipulationBoundaryFeedback;
            }
        }

        private static void ElementOnManipulationBoundaryFeedback(object? sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}
