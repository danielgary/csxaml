using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace Csxaml.Runtime;

internal static class CommonElementEventBinder
{
    public static void Apply(
        FrameworkElement control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore,
        bool bindKeyDown = true)
    {
        BindLoaded(control, node, bindingStore);
        if (bindKeyDown)
        {
            BindKeyDown(control, node, bindingStore);
        }

        BindPointer(control, node, bindingStore, "OnPointerCanceled", add => control.PointerCanceled += add, remove => control.PointerCanceled -= remove);
        BindPointer(control, node, bindingStore, "OnPointerCaptureLost", add => control.PointerCaptureLost += add, remove => control.PointerCaptureLost -= remove);
        BindPointer(control, node, bindingStore, "OnPointerEntered", add => control.PointerEntered += add, remove => control.PointerEntered -= remove);
        BindPointer(control, node, bindingStore, "OnPointerExited", add => control.PointerExited += add, remove => control.PointerExited -= remove);
        BindPointer(control, node, bindingStore, "OnPointerMoved", add => control.PointerMoved += add, remove => control.PointerMoved -= remove);
        BindPointer(control, node, bindingStore, "OnPointerPressed", add => control.PointerPressed += add, remove => control.PointerPressed -= remove);
        BindPointer(control, node, bindingStore, "OnPointerReleased", add => control.PointerReleased += add, remove => control.PointerReleased -= remove);
        BindPointer(control, node, bindingStore, "OnPointerWheelChanged", add => control.PointerWheelChanged += add, remove => control.PointerWheelChanged -= remove);
    }

    private static void BindLoaded(
        FrameworkElement control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        NativeEventArgsBinder.Rebind<RoutedEventArgs>(
            node,
            bindingStore,
            "OnLoaded",
            handler =>
            {
                RoutedEventHandler typedHandler = (_, args) => handler(args);
                control.Loaded += typedHandler;
                return () => control.Loaded -= typedHandler;
            });
    }

    private static void BindKeyDown(
        FrameworkElement control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        NativeEventArgsBinder.Rebind<KeyRoutedEventArgs>(
            node,
            bindingStore,
            "OnKeyDown",
            handler =>
            {
                KeyEventHandler typedHandler = (_, args) => handler(args);
                control.AddHandler(UIElement.KeyDownEvent, typedHandler, handledEventsToo: true);
                return () => control.RemoveHandler(UIElement.KeyDownEvent, typedHandler);
            });
    }

    private static void BindPointer(
        FrameworkElement control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore,
        string name,
        Action<PointerEventHandler> add,
        Action<PointerEventHandler> remove)
    {
        NativeEventArgsBinder.Rebind<PointerRoutedEventArgs>(
            node,
            bindingStore,
            name,
            handler =>
            {
                PointerEventHandler typedHandler = (_, args) => handler(args);
                add(typedHandler);
                return () => remove(typedHandler);
            });
    }
}
