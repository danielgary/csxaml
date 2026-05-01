using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace Csxaml.Runtime;

internal sealed class RootPointerWheelBridge : IDisposable
{
    private readonly UIElement _rootElement;
    private readonly PointerEventHandler _handler;
    private bool _isDisposed;

    private RootPointerWheelBridge(UIElement rootElement)
    {
        _rootElement = rootElement;
        _handler = OnPointerWheelChanged;
        _rootElement.AddHandler(
            UIElement.PointerWheelChangedEvent,
            _handler,
            handledEventsToo: true);
    }

    public static RootPointerWheelBridge Attach(UIElement rootElement)
    {
        return new RootPointerWheelBridge(rootElement);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _rootElement.RemoveHandler(UIElement.PointerWheelChangedEvent, _handler);
    }

    private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs args)
    {
        if (args.Handled)
        {
            return;
        }

        var point = args.GetCurrentPoint(_rootElement);
        var scroller = WheelScrollTargetFinder.Find(_rootElement, point.Position);
        if (scroller is null)
        {
            return;
        }

        if (ScrollViewerWheelScroller.TryScroll(scroller, point.Properties.MouseWheelDelta))
        {
            args.Handled = true;
        }
    }
}
