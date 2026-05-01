using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Csxaml.Runtime;

internal sealed class SliderInteractionState
{
    private IDisposable? _renderDeferral;
    private bool _eventsAttached;

    public ControlledRangeInputState RangeInput { get; } = new();

    public void EnsureDragEvents(Slider control)
    {
        if (_eventsAttached)
        {
            return;
        }

        control.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(BeginDrag), handledEventsToo: true);
        control.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(EndDrag), handledEventsToo: true);
        control.AddHandler(UIElement.PointerCanceledEvent, new PointerEventHandler(EndDrag), handledEventsToo: true);
        control.AddHandler(UIElement.PointerCaptureLostEvent, new PointerEventHandler(EndDrag), handledEventsToo: true);
        _eventsAttached = true;
    }

    private void BeginDrag(object sender, PointerRoutedEventArgs args)
    {
        _renderDeferral ??= NativeEventRenderDeferral.Begin();
    }

    private void EndDrag(object sender, PointerRoutedEventArgs args)
    {
        _renderDeferral?.Dispose();
        _renderDeferral = null;
    }
}
